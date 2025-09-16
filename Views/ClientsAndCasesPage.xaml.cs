using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using Vakilaw.ViewModels;

namespace Vakilaw.Views;

public partial class ClientsAndCasesPage : ContentPage
{
	public ClientsAndCasesPage(ClientsAndCasesViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}