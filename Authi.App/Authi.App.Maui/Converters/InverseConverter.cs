using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace Authi.App.Maui.Converters
{
    public class InverseConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !(bool)(value ?? false);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !(bool)(value ?? false);
        }
    }
}
