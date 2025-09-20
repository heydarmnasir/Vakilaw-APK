using Mopups.Pages;
using Vakilaw.ViewModels;

namespace Vakilaw.Views.Popups;

public partial class LawyerInfoPopup : PopupPage
{
    public LawyerInfoPopup()
    {
        InitializeComponent();
        BindingContext = new LawyerInfoVM();
    }
}