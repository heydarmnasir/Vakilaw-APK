using Vakilaw.ViewModels;

namespace Vakilaw.Views;

public partial class TransactionsPage : ContentPage
{
	public TransactionsPage(TransactionsVM vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}