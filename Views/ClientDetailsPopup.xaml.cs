using Mopups.Pages;
using Vakilaw.Models;
using Vakilaw.ViewModels;

namespace Vakilaw.Views;

public partial class ClientDetailsPopup : PopupPage
{
	public ClientDetailsPopup(Client client, ClientService clientService)
	{
		InitializeComponent();
        BindingContext = new ClientDetailsPopupViewModel(client, clientService);
    }
}