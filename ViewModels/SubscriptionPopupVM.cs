using AsyncAwaitBestPractices;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using System.Collections.ObjectModel;
using Vakilaw.Models.Messages;
using Vakilaw.Services;
using Vakilaw.Views.Popups;

public partial class SubscriptionPopupVM : ObservableObject
{
    private readonly LicenseService _licenseService;

    [ObservableProperty] private string licenseKey;
    [ObservableProperty] private string selectedSubscription;
    [ObservableProperty] private bool isTrialActive;
    [ObservableProperty] private DateTime trialEndDate;

    [ObservableProperty] private bool isProcessing;
    [ObservableProperty] private string errorMessage;

    [ObservableProperty] private string deviceId; // NEW

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public string ActivateButtonText => IsProcessing ? "در حال پردازش..." : "فعال‌سازی";

    public ObservableCollection<string> SubscriptionOptions { get; } = new()
    {
        "1 ماهه", "3 ماهه", "سالانه"
    };

    public SubscriptionPopupVM(LicenseService licenseService)
    {
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
        DeviceId = DeviceHelper.GetDeviceId(); // نمایش DeviceId برای کپی
        InitializeTrialStateAsync().SafeFireAndForget();
    }

    [RelayCommand]
    private async Task CopyDeviceIdAsync()
    {
        try
        {
            await Clipboard.SetTextAsync(DeviceId);
            await Toast.Make("DeviceId کپی شد", ToastDuration.Short).Show();
        }
        catch
        {
            await Toast.Make("خطا در کپی DeviceId", ToastDuration.Short).Show();
        }
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

    private bool _isPopupClosing = false;

    [RelayCommand]
    private async Task ActivateLicenseAsync()
    {
        if (IsProcessing) return;
        IsProcessing = true;
        ErrorMessage = string.Empty;

        try
        {
            var deviceId = DeviceHelper.GetDeviceId();

            if (!string.IsNullOrWhiteSpace(LicenseKey))
            {
                var cleanedLicenseKey = new string(LicenseKey.Where(c => !char.IsWhiteSpace(c)).ToArray());

                try
                {
                    bool result = await _licenseService.ActivateSignedLicenseAsync(cleanedLicenseKey);
                    if (result)
                    {
                        // آپدیت Preferences (ممکنه LicenseService هم قبلاً انجام داده باشه)
                        var (start, end, type) = _licenseService.GetCurrentLicenseInfo();
                        Preferences.Set("IsSubscriptionActive", true);
                        Preferences.Set("SubscriptionType", type);
                        Preferences.Set("LicenseStart", start.Ticks);
                        Preferences.Set("LicenseEnd", end.Ticks);

                        // ارسال پیام به ViewModel پاپ‌آپ برای Refresh اطلاعات
                        WeakReferenceMessenger.Default.Send(new LicenseActivatedMessage(true));
                        await Toast.Make("لایسنس فعال شد", ToastDuration.Long).Show();
                      

                        if (!_isPopupClosing)
                        {
                            try
                            {
                                _isPopupClosing = true;
                                await MopupService.Instance.PopAllAsync();
                            }
                            catch
                            {
                                // نادیده گرفتن خطا
                            }
                            finally
                            {
                                _isPopupClosing = false;
                            }
                        }
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // نمایش دلیل دقیق شکست لایسنس
                    ErrorMessage = $"خطا در فعال‌سازی لایسنس: {ex.Message}";
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(SelectedSubscription))
            {
                ErrorMessage = "لطفاً نوع اشتراک را انتخاب کنید یا کلید لایسنس را وارد کنید";
                return;
            }

            ErrorMessage = $"برای خرید {SelectedSubscription}، لطفاً کلید دستگاه را کپی کرده و برای توسعه‌دهنده ارسال کنید.";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task ActiveLicenseTutorialAsync()
    {
        var popup = new ActiveLicenseTutorialPopup();
        await MopupService.Instance.PushAsync(popup);
    }

    [RelayCommand]
    private async Task ClosePopupAsync()
    {
        if (!IsProcessing)
            await MopupService.Instance.PopAsync();
    }
}