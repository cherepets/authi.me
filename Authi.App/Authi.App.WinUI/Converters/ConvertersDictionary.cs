using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Authi.App.WinUI.Converters
{
    public partial class ConvertersDictionary : ResourceDictionary
    {
        [RequiresUnreferencedCode("Uses Assembly.DefinedTypes to find all converters.")]
        public ConvertersDictionary()
        {
            var assembly = typeof(ConvertersDictionary).GetTypeInfo().Assembly;
            var baseType = typeof(IValueConverter).GetTypeInfo();
            var converters = assembly.DefinedTypes.Where(baseType.IsAssignableFrom).ToList();
            foreach (var converter in converters)
            {
                var name = converter.Name.Replace("Converter", string.Empty);
                var value = Activator.CreateInstance(converter.AsType());
                this[name] = value;
            }
        }
    }
}
