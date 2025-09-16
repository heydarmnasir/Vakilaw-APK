using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using Vakilaw.Models;
using Vakilaw.Views;

namespace Vakilaw.ViewModels
{
    public partial class ClientDetailsPopupViewModel : ObservableObject
    {
        [ObservableProperty] private string fullName;
        [ObservableProperty] private string nationalCode;
        [ObservableProperty] private string address;
        [ObservableProperty] private string description;
      
        public ClientDetailsPopupViewModel(Client client)
        {
            FullName = client.FullName;
            NationalCode = client.NationalCode;
            Address = client.Address;
            Description = client.Description;         
        }

        [RelayCommand]
        private async Task Close()
        {
            await MopupService.Instance.PopAsync();
        }
    }
}