using Mopups.Pages;
using Vakilaw.Models;
using Vakilaw.ViewModels;

namespace Vakilaw.Views.Popups;

public partial class CaseDetailsPopup : PopupPage
{
	public CaseDetailsPopup(Case caseItem, CaseService caseService)
	{
		InitializeComponent();
        BindingContext = new CaseDetailsPopupViewModel(caseItem, caseService);
    }
}