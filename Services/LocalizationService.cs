using CommunityToolkit.Mvvm.ComponentModel;
using System.Globalization;
using System.Resources;

namespace Vakilaw.Services
{
    public class LocalizationService : ObservableObject
    {
        private static LocalizationService _instance;
        public static LocalizationService Instance => _instance ??= new LocalizationService();

        private ResourceManager _resourceManager;

        public event Action? LanguageChanged;

        private LocalizationService()
        {
            _resourceManager = new ResourceManager("Vakilaw.Resources.Localization.AppResources", typeof(LocalizationService).Assembly);
            LoadSavedLanguage();
        }

        public string this[string key]
        {
            get => _resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
        }

        public void SetLanguage(string langCode)
        {
            CultureInfo culture = new CultureInfo(langCode);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            Preferences.Set("AppLanguage", langCode);

            LanguageChanged?.Invoke();
        }

        public void LoadSavedLanguage()
        {
            var langCode = Preferences.Get("AppLanguage", "fa");
            CultureInfo culture = new CultureInfo(langCode);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        public void UpdateFlowDirection(Page page)
        {
            page.FlowDirection = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft
                ? FlowDirection.RightToLeft
                : FlowDirection.LeftToRight;
        }
    }
}