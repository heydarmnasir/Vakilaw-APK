using Mopups.Pages;
using Vakilaw.Models;
using Vakilaw.ViewModels;

namespace Vakilaw.Views;

public partial class ClientDetailsPopup : PopupPage
{
	public ClientDetailsPopup(Client client)
	{
		InitializeComponent();
        BindingContext = new ClientDetailsPopupViewModel(client);
    }
}