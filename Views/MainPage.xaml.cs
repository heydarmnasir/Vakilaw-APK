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

            // رجیستر پنل‌ها برای انیمیشن
            vm.LawyersListPanelRef = LawyersListPanel;
            vm.BookmarkPanelRef = BookmarkPanel;
            vm.SettingsPanelRef = SettingsPanel;

            LocalizationService.Instance.UpdateFlowDirection(this);
            LocalizationService.Instance.LanguageChanged += () =>
            {
                LocalizationService.Instance.UpdateFlowDirection(this);
            };
        }    
    }  
}