using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using System.Globalization;
using Vakilaw.Helpers;

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
    }

    private void LoadUserInfo()
    {
        FullName = Preferences.Get("LawyerFullName", "");
        Phone = Preferences.Get("UserPhone", "");
        License = Preferences.Get("LawyerLicense", "");

        var plan = Preferences.Get("SubscriptionPlan", "رایگان (Trial)");
        var endDateString = Preferences.Get("SubscriptionEndDate", "");
        var isActive = Preferences.Get("IsSubscriptionActive", false);

        if (!string.IsNullOrEmpty(endDateString) &&
        DateTime.TryParse(endDateString, null, DateTimeStyles.RoundtripKind, out var endDate))
        {
            var persianDate = PersianDateHelper.ToPersianDate(endDate);
            if (isActive && endDate >= DateTime.Now)
                SubscriptionStatus = $"فعال تا {persianDate} - {plan}";
            else
                SubscriptionStatus = $"منقضی شده - {plan}";
        }
        else
        {
            // اگر تاریخ معتبر نبود، حداقل نام اشتراک نشان داده شود
            SubscriptionStatus = plan;
        }
    }

    [RelayCommand]
    private async Task CloseAsync()
    {
        await MopupService.Instance.PopAsync();
    }
}