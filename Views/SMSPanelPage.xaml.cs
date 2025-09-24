using Vakilaw.ViewModels;

namespace Vakilaw.Views;

public partial class SMSPanelPage : ContentPage
{
	public SMSPanelPage(ClientService clientService, SmsService smsService)
	{
		InitializeComponent();
        BindingContext = new SmsPanelVM(clientService, smsService);
    }
}