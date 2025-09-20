using Mopups.Pages;
using Vakilaw.ViewModels;
using Vakilaw.Services;

namespace Vakilaw.Views.Popups;

public partial class SubscriptionPopup : PopupPage
{
    public SubscriptionPopup(LicenseService licenseService)
    {
        InitializeComponent();
        BindingContext = new SubscriptionPopupVM(licenseService);
    }
}