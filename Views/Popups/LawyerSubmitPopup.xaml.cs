using Mopups.Pages;
using Vakilaw.Services;
using Vakilaw.ViewModels;

namespace Vakilaw.Views.Popups;

public partial class LawyerSubmitPopup : PopupPage
{
    public LawyerSubmitPopup(UserService userService, SmsService otpService, LicenseService licenseService, string deviceId)
    {
        InitializeComponent();
        BindingContext = new LawyerSubmitVM(userService, otpService, licenseService, deviceId);
    }
}