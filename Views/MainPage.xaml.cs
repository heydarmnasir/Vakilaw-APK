using Vakilaw.Services;
using Vakilaw.ViewModels;

namespace Vakilaw.Views
{
    public partial class MainPage : ContentPage
    {     
        public MainPage(MainPageVM vm)
        {
            InitializeComponent();
            BindingContext = vm;

            LocalizationService.Instance.UpdateFlowDirection(this);
            LocalizationService.Instance.LanguageChanged += () =>
            {
                LocalizationService.Instance.UpdateFlowDirection(this);
            };
        }

        double lastScrollY = 0;
        bool headerElevated = false;

        private async void LawyersList_Scrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            if (e.VerticalOffset > 5 && !headerElevated)
            {
                headerElevated = true;

                // انیمیشن ظاهر شدن سایه
                await Task.WhenAll(
                    HeaderGrid.TranslateTo(0, 0, 150, Easing.CubicOut),
                    HeaderGrid.FadeTo(1, 150, Easing.CubicInOut)
                );

                HeaderGrid.Shadow = new Shadow
                {
                    Brush = Brush.Black,
                    Opacity = 0.2f,
                    Radius = 5,
                    Offset = new Point(0, 2)
                };
            }
            else if (e.VerticalOffset <= 5 && headerElevated)
            {
                headerElevated = false;

                // حذف سایه با انیمیشن نرم
                HeaderGrid.Shadow = new Shadow
                {
                    Brush = Brush.Black,
                    Opacity = 0f,
                    Radius = 0,
                    Offset = new Point(0, 0)
                };
            }
            lastScrollY = e.VerticalOffset;
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
            else if (LawyersListPanel.IsVisible == true)
            {
                await LawyersListPanel.TranslateTo(this.Width, 0, 400, Easing.CubicIn);
                LawyersListPanel.IsVisible = false;
            }
        }

        private async void LawyersList_Tapped(object sender, TappedEventArgs e)
        {
            if (LawyersListPanel.IsVisible == false)
            {
                LawyersListPanel.TranslationX = this.Width;
                LawyersListPanel.IsVisible = true;
                await LawyersListPanel.TranslateTo(0, 0, 400, Easing.CubicOut);
            }
            else
            {
                await LawyersListPanel.TranslateTo(this.Width, 0, 400, Easing.CubicIn);
                LawyersListPanel.IsVisible = false;
            }
        }
        
        private async void OpenHamiVakilSite_Clicked(object sender, EventArgs e)
        {
            await Launcher.OpenAsync(new Uri("https://search-hamivakil.ir/"));
        }
    }
}