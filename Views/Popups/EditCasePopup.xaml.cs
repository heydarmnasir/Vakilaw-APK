using Mopups.Pages;
using Vakilaw.Models;
using Vakilaw.ViewModels;

namespace Vakilaw.Views.Popups;

public partial class EditCasePopup : PopupPage
{
    // بیرون‌دادن رویداد تا کسی که popup رو باز کرده بتونه بهش گوش بده
    public event Action<Case> CaseUpdated;

    public EditCasePopup(Case caseItem, CaseService caseService)
    {
        InitializeComponent();
        var vm = new EditCasePopupViewModel(this, caseItem, caseService);
        BindingContext = vm;
    }

    public void RaiseCaseUpdated(Case updated) => CaseUpdated?.Invoke(updated);
}