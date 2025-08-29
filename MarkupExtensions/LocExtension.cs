using Vakilaw.Services;

namespace Vakilaw.MarkupExtensions
{
    [ContentProperty(nameof(TextKey))]
    public class LocExtension : IMarkupExtension
    {
        public string TextKey { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(TextKey))
                return string.Empty;

            var valueProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            var targetObject = valueProvider?.TargetObject as BindableObject;
            var targetProperty = valueProvider?.TargetProperty as BindableProperty;

            if (targetObject != null && targetProperty != null)
            {
                void UpdateText() => targetObject.SetValue(targetProperty, LocalizationService.Instance[TextKey]);
                // مقدار اولیه
                UpdateText();
                // آپدیت هنگام تغییر زبان
                LocalizationService.Instance.LanguageChanged += UpdateText;
            }
            return LocalizationService.Instance[TextKey];
        }
    }
}