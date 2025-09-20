using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IPE.SmsIrClient;
using Mopups.Services;
using Vakilaw.Models.Messages;
using Vakilaw.Services;
using Vakilaw.Views.Popups;

namespace Vakilaw.ViewModels;

public partial class LawyerSubmitVM : ObservableObject
{
    private readonly UserService _userService;
    private readonly OtpService _otpService;
    private readonly LicenseService _licenseService;
    private readonly string _deviceId;

    private string _currentOtp;
    private CancellationTokenSource _otpTimerCts;

    public LawyerSubmitVM(UserService userService, OtpService otpService, LicenseService licenseService, string deviceId)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
        _deviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
    }

    #region Properties
    [ObservableProperty] private string fullName;
    [ObservableProperty] private string phoneNumber;
    [ObservableProperty] private string licenseNumber;
    [ObservableProperty] private string enteredOtp;
    [ObservableProperty] private bool isOtpSent;
    [ObservableProperty] private bool isTrialActive;
    [ObservableProperty] private DateTime trialEndDate;
    [ObservableProperty] private bool isBusy;

    // Properties for OTP resend timer
    [ObservableProperty] private bool canResendOtp = true;
    [ObservableProperty] private int resendOtpCountdown;

    // پراپرتی فقط خواندنی برای متن دکمه
    public string OtpButtonText =>
        CanResendOtp ? "ارسال کد" : $"ارسال مجدد ({ResendOtpCountdown}s)";
    #endregion

    partial void OnCanResendOtpChanged(bool value)
    {
        OnPropertyChanged(nameof(OtpButtonText));
    }

    partial void OnResendOtpCountdownChanged(int value)
    {
        OnPropertyChanged(nameof(OtpButtonText));
    }


    #region Commands
    [RelayCommand]
    private async Task SendOtpAsync()
    {
        if (IsBusy || !CanResendOtp) return;
        IsBusy = true;

        if (string.IsNullOrWhiteSpace(FullName) ||
            string.IsNullOrWhiteSpace(PhoneNumber) ||
            string.IsNullOrWhiteSpace(LicenseNumber))
        {
            await Toast.Make("لطفاً همه فیلدها را پر کنید!", ToastDuration.Short).Show();
            IsBusy = false;
            return;
        }

        if (PhoneNumber.Trim().Length != 11 || LicenseNumber.Trim().Length != 4)
        {
            await Toast.Make("شماره موبایل یا پروانه نامعتبر است!", ToastDuration.Short).Show();
            IsBusy = false;
            return;
        }

        try
        {
            _currentOtp = await _otpService.SendOtpAsync(PhoneNumber, FullName);

            if (string.IsNullOrEmpty(_currentOtp))
            {
                await Toast.Make("ارسال پیامک انجام شد اما کدی دریافت نشد.", ToastDuration.Short).Show();
                IsBusy = false;
                return;
            }

            IsOtpSent = true;
            CanResendOtp = false;
            ResendOtpCountdown = 60; // 60 ثانیه

            _otpTimerCts?.Cancel();
            _otpTimerCts = new CancellationTokenSource();

            _ = StartOtpCountdownAsync(_otpTimerCts.Token);

            await Toast.Make("کد تایید ارسال شد", ToastDuration.Short).Show();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("SendOtpAsync Exception: " + ex);
            await Toast.Make("خطا در ارسال کد: " + ex.Message, ToastDuration.Short).Show();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private const string LawyerRole = "Lawyer";

    [RelayCommand]
    private async Task VerifyOtpAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        if (string.IsNullOrWhiteSpace(EnteredOtp) || EnteredOtp.Trim() != _currentOtp)
        {
            await Toast.Make("کد تایید اشتباه است", ToastDuration.Short).Show();
            IsBusy = false;
            return;
        }

        try
        {
            // ثبت نام کاربر
            var user = await _userService.RegisterUserAsync(FullName, PhoneNumber, LawyerRole, LicenseNumber);

            // ذخیره اطلاعات پایه کاربر
            Preferences.Set("IsLawyerRegistered", true);
            Preferences.Set("UserId", user.Id);
            Preferences.Set("LawyerFullName", user.FullName);
            Preferences.Set("LawyerLicense", user.LicenseNumber);
            Preferences.Set("UserRole", user.Role);
            Preferences.Set("UserPhone", user.PhoneNumber);

            // ایجاد Trial 14 روزه
            var license = await _licenseService.CreateOrGetTrialAsync(_deviceId, PhoneNumber);
            TrialEndDate = license.EndDate;
            IsTrialActive = license.IsActive;

            // ✅ ذخیره تاریخ با فرمت استاندارد Round-trip
            Preferences.Set("SubscriptionPlan", "رایگان (Trial)");
            Preferences.Set("SubscriptionEndDate", license.EndDate.ToString("o"));
            Preferences.Set("IsSubscriptionActive", license.IsActive);

            // برای سازگاری با کدهای قدیمی (اختیاری)
            Preferences.Set("TrialEndDate", license.EndDate.ToString("o"));
            Preferences.Set("IsTrialActive", license.IsActive);

            // ارسال پیام به سایر ViewModel ها
            WeakReferenceMessenger.Default.Send(new LawyerRegisteredMessage(user.FullName, user.LicenseNumber));
            WeakReferenceMessenger.Default.Send(new LicenseActivatedMessage(true));

            // پاک کردن OTP برای امنیت
            EnteredOtp = string.Empty;
            _currentOtp = null;

            await MopupService.Instance.PopAsync();

            // ✅ باز کردن بلافاصله پاپ‌آپ اطلاعات کاربری و اشتراک
            var popup = new LawyerInfoPopup();
            await MopupService.Instance.PushAsync(popup);

            await Toast.Make("ثبت نام و فعال‌سازی Trial 14 روزه با موفقیت انجام شد ✅", ToastDuration.Long).Show();

            SmsIr smsIr = new SmsIr("nAhAuhG1zeatLYl8giJtgTgTc9L1788EQbckz7iGd1uYUz28");
            var bulkSendResult = smsIr.BulkSendAsync(30007732011420, $"وکیل گرامی {user.FullName}\nثبت نام شما در اپلیکیشن حقوقی وکیلاو را تبریک عرض میکنم\nجهت راهنمایی و یا ارسال درخواست، به شماره زیر:\n+989023349043 پیغام بدهید \n سربلند و پیروز باشید", new string[] { user.PhoneNumber });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("VerifyOtpAsync Exception: " + ex);
            await Toast.Make("خطا در ثبت نام", ToastDuration.Short).Show();
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region OTP Timer
    private async Task StartOtpCountdownAsync(CancellationToken token)
    {
        try
        {
            while (ResendOtpCountdown > 0 && !token.IsCancellationRequested)
            {
                await Task.Delay(1000, token);
                ResendOtpCountdown--;
            }
        }
        catch (TaskCanceledException) { }
        finally
        {
            if (!token.IsCancellationRequested)
                CanResendOtp = true;
        }
    }
    #endregion

    [RelayCommand]
    private async Task ClosePopupAsync()
    {
        await MopupService.Instance.PopAsync();
    }
}