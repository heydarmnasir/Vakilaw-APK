using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vakilaw.Services;

namespace Vakilaw.ViewModels
{
    public partial class BuySubscriptionVM : ObservableObject
    {
        private readonly SubscriptionService _subscriptionService;

        public BuySubscriptionVM(SubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [RelayCommand]
        private async Task Buy3MonthAsync() => await BuyAsync("3Month");

        [RelayCommand]
        private async Task Buy6MonthAsync() => await BuyAsync("6Month");

        [RelayCommand]
        private async Task BuyYearlyAsync() => await BuyAsync("Yearly");

        private async Task BuyAsync(string type)
        {
            int userId = Preferences.Get("UserId", 0);

            // اینجا بعداً وصل میشه به زرین پال
            _subscriptionService.AddOrRenewSubscription(userId, type, "TestTrackingCode");

            await Toast.Make("اشتراک شما با موفقیت فعال شد ✅").Show();

            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}