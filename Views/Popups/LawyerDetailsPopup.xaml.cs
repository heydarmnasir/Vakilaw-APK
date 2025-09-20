using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Mopups.Pages;
using Mopups.Services;

namespace Vakilaw.Views.Popups;

public partial class LawyerDetailsPopup : PopupPage
{
    private readonly string _phoneNumber;
    
    public LawyerDetailsPopup(string fullName, string phoneNumber, string description)
    {
        InitializeComponent();
        FullNameLabel.Text = fullName;
        PhoneLabel.Text = phoneNumber;
        DescriptionLabel.Text = description;
        _phoneNumber = phoneNumber;
    }

    private async void ClosePopup(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }

    private async void CallLawyer(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_phoneNumber))
        {
            try
            {
                // باز کردن شماره‌گیر موبایل
                await Launcher.OpenAsync(new Uri($"tel:{_phoneNumber}"));
            }
            catch (Exception)
            {
                await Toast.Make("امکان برقراری تماس وجود ندارد!", ToastDuration.Long, 14).Show();            
            }
        }
    }

    private async void SendMessageToLawyer(object sender, EventArgs e)
    {
        try
        {
            var message = new SmsMessage
            {
                Body = "سلام، وقت بخیر",
                Recipients = new List<string> { _phoneNumber }
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

    private async void CopyPhoneNumber_Clicked(object sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync(_phoneNumber);
        await Toast.Make("شماره تلفن کپی شد", ToastDuration.Short, 14).Show();       
    }
}