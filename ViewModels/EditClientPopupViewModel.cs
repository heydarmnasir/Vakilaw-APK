using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using Vakilaw.Models;
using Vakilaw.Views.Popups;

namespace Vakilaw.ViewModels;

public partial class EditClientPopupViewModel : ObservableObject
{
    private readonly EditClientPopup _popup;
    private readonly ClientService _clientService;
    private readonly Client _client; // مرجع اصلی پرونده
  
    [ObservableProperty] private string phoneNumber;
    [ObservableProperty] private string address;
    [ObservableProperty] private string description;

    public EditClientPopupViewModel(EditClientPopup popup, Client clientItem, ClientService clientService)
    {
        _popup = popup;
        _clientService = clientService;
        _client = clientItem;

        // مقداردهی اولیه
        PhoneNumber = _client.PhoneNumber;
        Address = _client.Address;     
        Description = _client.Description;     
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            await Toast.Make("لطفاً تلفن همراه را وارد کنید!", ToastDuration.Short).Show();
            return;
        }

        var phonenumberLength = PhoneNumber.Trim().Length;

        if (phonenumberLength > 11 || phonenumberLength < 11)
        {
            await Toast.Make("تلفن همراه نامعتبر است!", ToastDuration.Short).Show();
            return;
        }

        // بروزرسانی نمونه اصلی
        _client.PhoneNumber = PhoneNumber;
        _client.Address = Address;
        _client.Description = Description;
    
        // ذخیره در DB
        await _clientService.UpdateClient(_client);

        // اطلاع به بازکننده popup
        _popup.RaiseClientUpdated(_client);

        // بستن popup
        await MopupService.Instance.PopAsync();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await MopupService.Instance.PopAsync();
    }  
}