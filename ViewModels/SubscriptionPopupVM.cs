using AsyncAwaitBestPractices;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using System.Collections.ObjectModel;
using Vakilaw.Models;
using Vakilaw.Models.Messages;
using Vakilaw.Services;

public partial class SubscriptionPopupVM : ObservableObject
{
    private readonly LicenseService _licenseService;

    [ObservableProperty] private string licenseKey;
    [ObservableProperty] private string selectedSubscription;
    [ObservableProperty] private bool isTrialActive;
    [ObservableProperty] private DateTime trialEndDate;

    [ObservableProperty] private bool isProcessing;
    [ObservableProperty] private string errorMessage;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public string ActivateButtonText => IsProcessing ? "در حال پردازش..." : "فعال‌سازی";

    public ObservableCollection<string> SubscriptionOptions { get; } = new()
    {
        "3 ماهه", "6 ماهه", "سالانه"
    };

    public SubscriptionPopupVM(LicenseService licenseService)
    {
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
        InitializeTrialStateAsync().SafeFireAndForget();
    }

    private async Task InitializeTrialStateAsync()
    {
        try
        {
            var deviceId = DeviceHelper.GetDeviceId();
            var trial = await _licenseService.GetActiveLicenseAsync(deviceId);
            if (trial != null && trial.SubscriptionType == "Trial" && trial.EndDate > DateTime.Now)
            {
                IsTrialActive = true;
                TrialEndDate = trial.EndDate;
            }
        }
        catch
        {
            // در صورت خطا، تنها از UI جلوگیری می‌کنیم، نیازی به throw نیست
        }
    }

    [RelayCommand]
    private async Task ActivateLicenseAsync()
    {
        IsProcessing = true;
        ErrorMessage = string.Empty;

        try
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
                    return;
                }
                else
                {
                    ErrorMessage = "لایسنس نامعتبر است";
                    return;
                }
            }

            // بررسی انتخاب نوع اشتراک
            if (string.IsNullOrWhiteSpace(SelectedSubscription))
            {
                ErrorMessage = "لطفاً نوع اشتراک را انتخاب کنید";
                return;
            }

            int months = SelectedSubscription switch
            {
                "3 ماهه" => 3,
                "6 ماهه" => 6,
                "سالانه" => 12,
                _ => 0
            };

            if (months == 0)
            {
                ErrorMessage = "نوع اشتراک نامعتبر است";
                return;
            }

            // بررسی اشتراک فعال قبلی
            var existing = await _licenseService.GetActiveLicenseAsync(deviceId);
            if (existing != null && existing.EndDate > DateTime.Now)
            {
                ErrorMessage = $"اشتراک قبلی هنوز فعال است تا {existing.EndDate:yyyy/MM/dd}";
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
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task ClosePopupAsync()
    {
        if (!IsProcessing)
            await MopupService.Instance.PopAsync();
    }
}