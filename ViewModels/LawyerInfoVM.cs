using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Mopups.Services;
using System.Globalization;
using Vakilaw.Helpers;
using Vakilaw.Models.Messages;

namespace Vakilaw.ViewModels;

public partial class LawyerInfoVM : ObservableObject
{
    [ObservableProperty] private string fullName;
    [ObservableProperty] private string phone;
    [ObservableProperty] private string license;
    [ObservableProperty] private string subscriptionStatus;

    public LawyerInfoVM()
    {
        LoadUserInfo();

        // ثبت پیام برای بروزرسانی بعد از فعال‌سازی لایسنس
        WeakReferenceMessenger.Default.Register<LicenseActivatedMessage>(this, (r, m) =>
        {
            if (m.IsActivated)
                LoadUserInfo();
        });
    }

    private void LoadUserInfo()
    {
        FullName = Preferences.Get("LawyerFullName", "");
        Phone = Preferences.Get("UserPhone", "");
        License = Preferences.Get("LawyerLicense", "");

        var plan = Preferences.Get("SubscriptionPlan", "Trial");
        var endDateString = Preferences.Get("SubscriptionEndDate", "");
        var isActive = Preferences.Get("IsSubscriptionActive", false);

        if (!string.IsNullOrWhiteSpace(endDateString) &&
            DateTime.TryParse(endDateString, null, DateTimeStyles.RoundtripKind, out var endDate))
        {
            var persianDate = PersianDateHelper.ToPersianDate(endDate);

            if (isActive && endDate >= DateTime.Now)
                SubscriptionStatus = $"فعال تا {persianDate} - {GetPlanTitle(plan)}";
            else
                SubscriptionStatus = $"منقضی شده - {GetPlanTitle(plan)}";
        }
        else
        {
            // اگر تاریخ معتبر نبود، فقط پلن را نشان بده
            SubscriptionStatus = GetPlanTitle(plan);
        }
    }

    /// <summary>
    /// تبدیل مقدار ذخیره شده پلن (مثلا Trial, 1Month, 3Month, 1Year) به متن فارسی برای نمایش
    /// </summary>
    private static string GetPlanTitle(string plan) => plan switch
    {
        "Trial" => "رایگان (Trial)",
        "1Month" => "اشتراک 1 ماهه",
        "3Month" => "اشتراک 3 ماهه",
        "1Year" => "اشتراک سالانه",
        _ => plan // اگر رشته ناشناخته بود همون مقدار خام نشون داده میشه
    };

    [RelayCommand]
    private async Task CloseAsync()
    {
        await MopupService.Instance.PopAsync();
    }
}