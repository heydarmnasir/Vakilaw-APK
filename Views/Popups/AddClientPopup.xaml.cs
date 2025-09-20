using Mopups.Pages;
using Vakilaw.Models;

namespace Vakilaw.Views.Popups;

public partial class AddClientPopup : PopupPage
{
    private readonly ClientService _clientService;
    public AddClientPopup(ClientService clientService)
    {
        InitializeComponent();
        _clientService = clientService;
        BindingContext = new ViewModels.AddClientPopupViewModel(this, _clientService);
    }

    // Event برای خروجی موکل جدید
    public event Action<Client> ClientCreated;

    public void RaiseClientCreated(Client newClient)
    {
        ClientCreated?.Invoke(newClient);
    }
}