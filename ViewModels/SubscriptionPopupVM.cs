using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Mopups.Services;
using Vakilaw.Services;
using Vakilaw.Models;
using CommunityToolkit.Mvvm.Messaging;
using Vakilaw.Models.Messages;

public partial class SubscriptionPopupVM : ObservableObject
{
    private readonly LicenseService _licenseService;

    [ObservableProperty] private string licenseKey;
    [ObservableProperty] private string selectedSubscription; // "3 ماهه", "6 ماهه", "سالانه"
    [ObservableProperty] private bool isTrialActive;
    [ObservableProperty] private DateTime trialEndDate;

    public SubscriptionPopupVM(LicenseService licenseService)
    {
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
        InitializeTrialState();
    }

    private async void InitializeTrialState()
    {
        var deviceId = DeviceHelper.GetDeviceId();
        var trial = await _licenseService.GetActiveLicenseAsync(deviceId);
        if (trial != null && trial.SubscriptionType == "Trial" && trial.EndDate > DateTime.Now)
        {
            isTrialActive = true;
            trialEndDate = trial.EndDate;
        }
    }

    [RelayCommand]
    private async Task ActivateLicenseAsync()
    {
        var deviceId = DeviceHelper.GetDeviceId();

        // فعال‌سازی با کلید لایسنس
        if (!string.IsNullOrWhiteSpace(LicenseKey))
        {
            var result = await _licenseService.ActivateLicenseAsync(deviceId, LicenseKey);
            if (result)
            {
                await Toast.Make("لایسنس فعال شد", ToastDuration.Short).Show();
                WeakReferenceMessenger.Default.Send(new LicenseActivatedMessage(true));
                await MopupService.Instance.PopAsync();
            }
            else
            {
                await Toast.Make("لایسنس نامعتبر است", ToastDuration.Short).Show();
            }
            return;
        }

        // بررسی انتخاب نوع اشتراک
        if (string.IsNullOrWhiteSpace(SelectedSubscription))
        {
            await Toast.Make("لطفاً نوع اشتراک را انتخاب کنید", ToastDuration.Short).Show();
            return;
        }

        // نگاشت نوع اشتراک به تعداد ماه‌ها
        int months = SelectedSubscription switch
        {
            "3 ماهه" => 3,
            "6 ماهه" => 6,
            "سالانه" => 12,
            _ => 0
        };

        if (months == 0)
        {
            await Toast.Make("نوع اشتراک نامعتبر است", ToastDuration.Short).Show();
            return;
        }

        // بررسی وجود اشتراک فعال قبلی
        var existing = await _licenseService.GetActiveLicenseAsync(deviceId);
        if (existing != null && existing.EndDate > DateTime.Now)
        {
            await Toast.Make($"اشتراک قبلی هنوز فعال است تا {existing.EndDate:yyyy/MM/dd}", ToastDuration.Short).Show();
            return;
        }

        // ایجاد اشتراک جدید
        var now = DateTime.Now;
        var license = new LicenseInfo
        {
            DeviceId = deviceId,
            UserPhone = Preferences.Get("UserPhone", ""),
            StartDate = now,
            EndDate = now.AddMonths(months),
            IsActive = true,
            SubscriptionType = SelectedSubscription
        };

        await _licenseService.AddLicenseAsync(license);
        await Toast.Make($"{SelectedSubscription} فعال شد", ToastDuration.Short).Show();
        WeakReferenceMessenger.Default.Send(new LicenseActivatedMessage(true));
        await MopupService.Instance.PopAsync();
    }

    [RelayCommand]
    private async Task ClosePopupAsync()
    {
        await Mopups.Services.MopupService.Instance.PopAsync();
    }
}