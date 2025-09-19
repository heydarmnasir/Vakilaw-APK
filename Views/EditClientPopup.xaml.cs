using Mopups.Pages;
using Vakilaw.Models;
using Vakilaw.ViewModels;

namespace Vakilaw.Views;

public partial class EditClientPopup : PopupPage
{
    // بیرون‌دادن رویداد تا کسی که popup رو باز کرده بتونه بهش گوش بده
    public event Action<Client> ClientUpdated;

    public EditClientPopup(Client clientItem, ClientService clientService)
    {
        InitializeComponent();
        var vm = new EditClientPopupViewModel(this, clientItem, clientService);
        BindingContext = vm;
    }

    public void RaiseClientUpdated(Client updated) => ClientUpdated?.Invoke(updated);
}