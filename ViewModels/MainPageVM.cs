using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Storage;
using Mopups.Services;
using Vakilaw.Services;
using Vakilaw.Views;
using static Vakilaw.Views.LawyerSubmitPopup;

namespace Vakilaw.ViewModels;

public partial class MainPageVM : ObservableObject
{
    private readonly UserService _userService;

    [ObservableProperty] private bool isLawyer;
    [ObservableProperty] private bool canRegisterLawyer;

    [ObservableProperty] private bool showRegisterLabel;
    [ObservableProperty] private bool lawyerLicensevisibility = false;
    [ObservableProperty] private string lawyerFullName;
    [ObservableProperty] private string lawyerLicense;

    public MainPageVM(UserService userService)
    {
        _userService = userService;

        LoadUserState();

        // گوش دادن به پیام ثبت وکیل
        WeakReferenceMessenger.Default.Register<LawyerRegisteredMessage>(this, (r, m) =>
        {
            IsLawyer = true;
            CanRegisterLawyer = false;
            ShowRegisterLabel = false;
            LawyerLicensevisibility = true;
            LawyerFullName = m.Value;
            LawyerLicense = m.LicenseNumber;
        });
    }

    private void LoadUserState()
    {
        var role = Preferences.Get("UserRole", "Unknown");
        var isRegistered = Preferences.Get("IsLawyerRegistered", false);

        if (role == "Unknown")
        {
            IsLawyer = false;
            CanRegisterLawyer = true;
            ShowRegisterLabel = true;
            LawyerLicensevisibility = false;
        }
        else
        {
            IsLawyer = role == "Lawyer" && isRegistered;
            CanRegisterLawyer = !isRegistered;
            ShowRegisterLabel = !isRegistered;
            LawyerLicensevisibility = isRegistered;
            LawyerFullName = Preferences.Get("LawyerFullName", string.Empty);
            LawyerLicense = Preferences.Get("LawyerLicense", string.Empty);
        }
    }

    [RelayCommand]
    public async Task OpenLawyerPopup()
    {
        // ✍️ اگر هنوز ثبت‌نام نکرده، فرم ثبت‌نام رو نشون بده
        var popup = new LawyerSubmitPopup(_userService);
        await MopupService.Instance.PushAsync(popup);
    }
}
