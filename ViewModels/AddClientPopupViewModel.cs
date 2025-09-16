using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using Vakilaw.Models;
using Vakilaw.Services; // فرض می‌کنیم ClientService اینجا هست
using Vakilaw.Views; 

namespace Vakilaw.ViewModels;

public partial class AddClientPopupViewModel : ObservableObject
{
    private readonly AddClientPopup _popup;
    private readonly ClientService _clientService;

    public AddClientPopupViewModel(AddClientPopup popup, ClientService clientService)
    {
        _popup = popup;
        _clientService = clientService;
    }

    [ObservableProperty] private string fullName;
    [ObservableProperty] private string nationalCode;
    [ObservableProperty] private string phoneNumber;
    [ObservableProperty] private string address;
    [ObservableProperty] private string description;

    [RelayCommand]
    private async Task Save()
    {
        // 1️⃣ ایجاد موکل جدید
        var newClient = new Client
        {
            FullName = FullName,
            NationalCode = NationalCode,
            PhoneNumber = PhoneNumber,
            Address = Address,
            Description = Description
        };

        // 2️⃣ اضافه کردن مستقیم به سرویس / دیتابیس
        await _clientService.AddClient(newClient);

        // 3️⃣ اطلاع دادن به صفحه اصلی (wrapper اضافه شود)
        _popup.RaiseClientCreated(newClient);

        // 4️⃣ بستن پاپ‌آپ
        await MopupService.Instance.PopAsync();
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await MopupService.Instance.PopAsync();
    }
}