using Microsoft.Maui.Controls;
using System;
using System.Globalization;

namespace Authi.App.Maui.Converters
{
    public class InverseEnumToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var enumString = value?.ToString();
            return !string.Equals(enumString, (string?)parameter, StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
