using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Maui.Storage;
using Mopups.Pages;
using Mopups.Services;
using System.Data;
using Vakilaw.Services;

namespace Vakilaw.Views;

public partial class LawyerSubmitPopup : PopupPage
{
    private readonly UserService _userService;

    public LawyerSubmitPopup(UserService userService)
    {
        InitializeComponent();
        _userService = userService;
        BindingContext = this;
    }

    // Properties برای Binding
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string LicenseNumber { get; set; }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FullName) ||
            string.IsNullOrWhiteSpace(PhoneNumber) ||
            string.IsNullOrWhiteSpace(LicenseNumber))
        {
            await Toast.Make("لطفاً همه فیلدها را پر کنید!", ToastDuration.Short, 14).Show();
            return;          
        }
        if (PhoneNumber.Trim().Length != 11 || LicenseNumber.Trim().Length != 4)
        {
            await Toast.Make("تلفن همراه یا شماره پروانه نامعتبر است!", ToastDuration.Short, 14).Show();
            return;
        }

        try
        {
            // ثبت وکیل در دیتابیس
            var user = await _userService.RegisterUserAsync(FullName, PhoneNumber, "Lawyer", LicenseNumber);

            Preferences.Set("UserRole", "Lawyer");
            Preferences.Set("IsLawyerRegistered", true);                      
            Preferences.Set("LawyerFullName", FullName);
            Preferences.Set("LawyerLicense", LicenseNumber);

            // 🔹 ارسال پیام به MainPageVM برای بروزرسانی
            WeakReferenceMessenger.Default.Send(new LawyerRegisteredMessage(FullName,LicenseNumber));

            await MopupService.Instance.PopAsync(); // بستن پاپ‌آپ
            await Toast.Make("اطلاعات شما ثبت شد", ToastDuration.Short, 14).Show();          
            
        }
        catch (Exception ex)
        {
            await Toast.Make("خطا!", ToastDuration.Short, 14).Show();
        }
    }

    public class LawyerRegisteredMessage : ValueChangedMessage<string>
    {
        public string LicenseNumber { get; }
        public LawyerRegisteredMessage(string fullName, string licenseNumber) : base(fullName)
        {
            LicenseNumber = licenseNumber;
        }
    }
}