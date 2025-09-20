using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Mopups.Pages;

namespace Vakilaw.Views.Popups;

public partial class ActiveLicenseTutorialPopup : PopupPage
{
	public ActiveLicenseTutorialPopup()
	{
		InitializeComponent();
		CardNumber.Text = "5859-8312-3624-0544 \n بانک تجارت به نام حیدر محمدی نصیر";
	}

    private async void CopyCardNumber_Clicked(object sender, EventArgs e)
    {
        try
        {
            await Clipboard.SetTextAsync(CardNumber.Text);
            await Toast.Make("شماره کارت کپی شد", ToastDuration.Short).Show();
        }
        catch
        {
            await Toast.Make("خطا در کپی!", ToastDuration.Short).Show();
        }
    }

    private async void SendMessageToLawyer(object sender, EventArgs e)
    {
        try
        {
            var message = new SmsMessage
            {               
                Recipients = { "+989023349043" }
            };

            if (Sms.Default.IsComposeSupported)
                await Sms.Default.ComposeAsync(message);
            else
                await Toast.Make("امکان ارسال پیامک وجود ندارد!", ToastDuration.Long, 14).Show();
        }
        catch (Exception ex)
        {
            await Toast.Make($"مشکل در ارسال پیامک: {ex.Message}", ToastDuration.Long, 14).Show();
        }
    }
}