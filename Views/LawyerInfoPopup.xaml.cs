using Mopups.Pages;
using Vakilaw.ViewModels;

namespace Vakilaw.Views;

public partial class LawyerInfoPopup : PopupPage
{
    public LawyerInfoPopup()
    {
        InitializeComponent();
        BindingContext = new LawyerInfoVM();
    }
}