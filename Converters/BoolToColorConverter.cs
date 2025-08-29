using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Vakilaw.Converters;

public class BoolToColorConverter : IValueConverter
{
    public Color TrueColor { get; set; } = Colors.DodgerBlue;
    public Color FalseColor { get; set; } = Colors.LightGray;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? TrueColor : FalseColor;
        return FalseColor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Color c && c == TrueColor;
    }
}