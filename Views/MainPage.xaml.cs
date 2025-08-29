using Vakilaw.ViewModels;

namespace Vakilaw.Views
{
    public partial class MainPage : ContentPage
    {     
        public MainPage(MainPageVM vm)
        {
            InitializeComponent();
            BindingContext = vm;          
        }      
        // MainPage.xaml.cs
        private async void Settings_Tapped(object sender, EventArgs e)
        {
            if (SettingsPanel.IsVisible == false)
            {                
                SettingsPanel.TranslationX = this.Width;
                SettingsPanel.IsVisible = true;              
                await SettingsPanel.TranslateTo(0, 0, 400, Easing.CubicOut);
            }
            else
            {              
                await SettingsPanel.TranslateTo(this.Width, 0, 400, Easing.CubicIn);             
                SettingsPanel.IsVisible = false;
            }     
        }

        private async void AdlIran_Tapped(object sender, TappedEventArgs e)
        {
            adliran_bg.BackgroundColor = Microsoft.Maui.Graphics.Color.FromArgb("#343a40");
            await Task.Yield();
            await Launcher.OpenAsync(new Uri("https://adliran.ir/"));
            await Task.Delay(1000);
            adliran_bg.BackgroundColor = Colors.Transparent;         
        }

        private async void Home_Tapped(object sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("..");

            if (SettingsPanel.IsVisible == true)
            {
                await SettingsPanel.TranslateTo(this.Width, 0, 400, Easing.CubicIn);
                SettingsPanel.IsVisible = false;
            }
        }
    }
}