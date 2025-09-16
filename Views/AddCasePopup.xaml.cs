using Mopups.Pages;
using Vakilaw.Models;
using Vakilaw.ViewModels;

namespace Vakilaw.Views;

public partial class AddCasePopup : PopupPage
{
    public event Action<Case> CaseCreated;

    public AddCasePopup(Client client)
    {
        InitializeComponent();
        BindingContext = new AddCasePopupViewModel(this, client);
    }

    public void RaiseCaseCreated(Case newCase)
    {
        CaseCreated?.Invoke(newCase);
    }
}