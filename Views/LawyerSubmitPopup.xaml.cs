using Mopups.Pages;
using Vakilaw.Services;
using Vakilaw.ViewModels;

namespace Vakilaw.Views;

public partial class LawyerSubmitPopup : PopupPage
{
    public LawyerSubmitPopup(UserService userService, OtpService otpService, LicenseService licenseService, string deviceId)
    {
        InitializeComponent();
        BindingContext = new LawyerSubmitVM(userService, otpService, licenseService, deviceId);
    }
}