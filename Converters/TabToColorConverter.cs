using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace Vakilaw.Converters;
public class TabToColorConverter : IValueConverter
{
    public Color SelectedColor { get; set; } = Colors.Crimson;
    public Color DefaultColor { get; set; } = Colors.Transparent;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var selectedTab = value as string;
        var thisTab = parameter as string;
        return selectedTab == thisTab ? SelectedColor : DefaultColor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}