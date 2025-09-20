using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using System.Collections.ObjectModel;
using Vakilaw.Models;
using Vakilaw.Views.Popups;

namespace Vakilaw.ViewModels
{
    public partial class ClientDetailsPopupViewModel : ObservableObject
    {
        [ObservableProperty] private string fullName;
        [ObservableProperty] private string nationalCode;
        [ObservableProperty] private string phoneNumber;
        [ObservableProperty] private string address;
        [ObservableProperty] private string description;

        private readonly ClientService _clientService;
        private readonly Client _clientItem; // نگهداری نمونهٔ اصلی

        public ClientDetailsPopupViewModel(Client client, ClientService clientService)
        {
            _clientItem = client ?? throw new ArgumentNullException(nameof(client));
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));

            FullName = client.FullName;
            NationalCode = client.NationalCode;
            PhoneNumber = client.PhoneNumber;
            Address = client.Address;
            Description = client.Description;         
        }

        [RelayCommand]
        private async Task Edit()
        {
            // استفاده از نمونه موجود (نه نام نوع Case)
            var editPopup = new EditClientPopup(_clientItem, _clientService);

            // اگر خواستیم بعد از ویرایش UI این popup رو آپدیت کنیم به رویداد گوش میدیم:
            editPopup.ClientUpdated += updatedClient =>
            {
                // آپدیت پراپرتی‌ها در thread UI
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PhoneNumber = updatedClient.PhoneNumber;
                    Address = updatedClient.Address;                 
                    Description = updatedClient.Description;                   
                });
            };

            await MopupService.Instance.PushAsync(editPopup);
        }

        [RelayCommand]
        private async Task Close()
        {
            await MopupService.Instance.PopAsync();
        }
    }
}