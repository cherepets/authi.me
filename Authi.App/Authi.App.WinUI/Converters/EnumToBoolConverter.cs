using Microsoft.UI.Xaml.Data;
using System;

namespace Authi.App.WinUI.Converters
{
    public partial class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var enumString = value.ToString();
            return string.Equals(enumString, (string)parameter, StringComparison.InvariantCultureIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
