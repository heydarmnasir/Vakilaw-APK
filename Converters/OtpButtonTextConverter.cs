using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Vakilaw.Converters;
public class OtpButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool canResend && parameter is int countdown)
        {
            return canResend ? "ارسال کد" : $"ارسال مجدد ({countdown}s)";
        }
        return "ارسال کد";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}