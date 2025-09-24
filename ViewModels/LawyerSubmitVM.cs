using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using Vakilaw.Models.Messages;
using Vakilaw.Services;
using Vakilaw.Views.Popups;

namespace Vakilaw.ViewModels;

public partial class LawyerSubmitVM : ObservableObject
{
    private readonly UserService _userService;
    private readonly ISmsService _smsService; // ✅ از اینترفیس استفاده شود
    private readonly LicenseService _licenseService;
    private readonly string _deviceId;

    private string _currentOtp;
    private CancellationTokenSource _otpTimerCts;

    public LawyerSubmitVM(UserService userService, ISmsService smsService, LicenseService licenseService, string deviceId)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
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
    [ObservableProperty] private bool canResendOtp = true;
    [ObservableProperty] private int resendOtpCountdown;

    public string OtpButtonText => CanResendOtp ? "ارسال کد" : $"ارسال مجدد ({ResendOtpCountdown}s)";
    #endregion

    partial void OnCanResendOtpChanged(bool value) => OnPropertyChanged(nameof(OtpButtonText));
    partial void OnResendOtpCountdownChanged(int value) => OnPropertyChanged(nameof(OtpButtonText));

    #region Commands
    private const string LawyerRole = "Lawyer";

    [RelayCommand]
    private async Task SendOtpAsync()
    {
        if (IsBusy || !CanResendOtp) return;
        IsBusy = true;

        try
        {
            if (string.IsNullOrWhiteSpace(FullName) ||
                string.IsNullOrWhiteSpace(PhoneNumber) ||
                string.IsNullOrWhiteSpace(LicenseNumber))
            {
                await Toast.Make("لطفاً همه فیلدها را پر کنید!", ToastDuration.Short).Show();
                return;
            }

            if (PhoneNumber.Trim().Length != 11 || LicenseNumber.Trim().Length != 4)
            {
                await Toast.Make("شماره موبایل یا پروانه نامعتبر است!", ToastDuration.Short).Show();
                return;
            }

            // ارسال OTP واقعی
            _currentOtp = await _smsService.SendOtpAsync(PhoneNumber);
            System.Diagnostics.Debug.WriteLine($"OTP دریافت شد: {_currentOtp}");

            if (string.IsNullOrEmpty(_currentOtp))
            {
                await Toast.Make("کد دریافت نشد، لطفاً دوباره تلاش کنید.", ToastDuration.Short).Show();
                return;
            }

            IsOtpSent = true;
            CanResendOtp = false;
            ResendOtpCountdown = 60;

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

    [RelayCommand]
    private async Task VerifyOtpAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            if (string.IsNullOrWhiteSpace(EnteredOtp) || EnteredOtp.Trim() != _currentOtp)
            {
                await Toast.Make("کد تایید اشتباه است", ToastDuration.Short).Show();
                return;
            }

            var user = await _userService.RegisterUserAsync(FullName, PhoneNumber, LawyerRole, LicenseNumber);

            Preferences.Set("IsLawyerRegistered", true);
            Preferences.Set("UserId", user.Id);
            Preferences.Set("LawyerFullName", user.FullName);
            Preferences.Set("LawyerLicense", user.LicenseNumber);
            Preferences.Set("UserRole", user.Role);
            Preferences.Set("UserPhone", user.PhoneNumber);

            var license = await _licenseService.CreateOrGetTrialAsync(_deviceId, PhoneNumber);
            TrialEndDate = license.EndDate;
            IsTrialActive = license.IsActive;

            Preferences.Set("SubscriptionPlan", "رایگان (Trial)");
            Preferences.Set("SubscriptionEndDate", license.EndDate.ToString("o"));
            Preferences.Set("IsSubscriptionActive", license.IsActive);
            Preferences.Set("TrialEndDate", license.EndDate.ToString("o"));
            Preferences.Set("IsTrialActive", license.IsActive);

            WeakReferenceMessenger.Default.Send(new LawyerRegisteredMessage(user.FullName, user.LicenseNumber));
            WeakReferenceMessenger.Default.Send(new LicenseActivatedMessage(true));

            EnteredOtp = string.Empty;
            _currentOtp = null;

            await MopupService.Instance.PopAsync();
            await MopupService.Instance.PushAsync(new LawyerInfoPopup());

            await Toast.Make("ثبت نام و فعال‌سازی Trial 14 روزه با موفقیت انجام شد ✅", ToastDuration.Long).Show();

            // پیامک تبریک
            await _smsService.SendSingleAsync(
                user.PhoneNumber,
                $"وکیل گرامی {user.FullName}\nثبت نام شما در اپلیکیشن حقوقی وکیلاو تبریک عرض می‌شود.\n" +
                "جهت راهنمایی یا ارسال درخواست:\n+989023349043 پیام بدهید.\nسربلند و پیروز باشید.\nلغو11"
            );
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
    private async Task ClosePopupAsync() => await MopupService.Instance.PopAsync();
}