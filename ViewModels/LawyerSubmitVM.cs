using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using Vakilaw.Models.Messages;
using Vakilaw.Services;

namespace Vakilaw.ViewModels;

public partial class LawyerSubmitVM : ObservableObject
{
    private readonly UserService _userService;
    private readonly OtpService _otpService;
    private readonly SubscriptionService _subscriptionService; // 🔹 اضافه شد

    [ObservableProperty] private string fullName;
    [ObservableProperty] private string phoneNumber;
    [ObservableProperty] private string licenseNumber;
    [ObservableProperty] private string enteredOtp;
    [ObservableProperty] private bool isOtpSent;

    private string _currentOtp;

    public LawyerSubmitVM(
        UserService userService,
        OtpService otpService,
        SubscriptionService subscriptionService) // 🔹 اضافه شد
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));
        _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    }

    [RelayCommand]
    private async Task SendOtpAsync()
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

        if (_otpService == null)
        {
            await Toast.Make("سرویس ارسال پیامک مقداردهی نشده است.", ToastDuration.Short).Show();
            System.Diagnostics.Debug.WriteLine("OtpService is null in SendOtpAsync");
            return;
        }

        try
        {
            _currentOtp = await _otpService.SendOtpAsync(PhoneNumber, FullName);

            if (string.IsNullOrEmpty(_currentOtp))
            {
                await Toast.Make("ارسال پیامک انجام شد اما کدی دریافت نشد.", ToastDuration.Short).Show();
                return;
            }

            IsOtpSent = true;
            await Toast.Make("کد تایید ارسال شد", ToastDuration.Short).Show();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("SendOtpAsync Exception: " + ex);
            await Toast.Make("خطا در ارسال کد: " + ex.Message, ToastDuration.Short).Show();
        }
    }

    [RelayCommand]
    private async Task VerifyOtpAsync()
    {
        if (EnteredOtp != _currentOtp)
        {
            await Toast.Make("کد تایید اشتباه است", ToastDuration.Short).Show();
            return;
        }

        try
        {
            // ثبت نام کاربر
            var user = await _userService.RegisterUserAsync(FullName, PhoneNumber, "Lawyer", LicenseNumber);

            // 🔹 فعال‌سازی Trial بعد از ثبت نام موفق
            _subscriptionService.StartTrial(user.Id);

            // پیام به بخش‌های دیگر اپ
            WeakReferenceMessenger.Default.Send(new LawyerRegisteredMessage(FullName, LicenseNumber));

            await MopupService.Instance.PopAsync();
            await Toast.Make("ثبت نام و فعال‌سازی Trial 14 روزه با موفقیت انجام شد ✅").Show();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("VerifyOtpAsync Exception: " + ex);
            await Toast.Make("خطا در ثبت نام", ToastDuration.Short).Show();
        }
    }
}