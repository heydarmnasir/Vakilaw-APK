using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Vakilaw.Converters
{
    public class BoolToStarConverter : IValueConverter
    {
        // مقدار بازگشتی: نام فایل تصویر (در Resources/Images)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var marked = value as bool? ?? false;
            return marked ? "bookmark1.png" : "bookmarkoutline.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}