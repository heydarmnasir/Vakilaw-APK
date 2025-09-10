namespace Vakilaw.Views;

public partial class BuySubscriptionPage : ContentPage
{
	public BuySubscriptionPage(BuySubscriptionPage vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}