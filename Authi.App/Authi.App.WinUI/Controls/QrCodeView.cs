using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing.Imaging;
using System.IO;
using ZXing;
using ZXing.Windows.Compatibility;
using Image = Microsoft.UI.Xaml.Controls.Image;

namespace Authi.App.WinUI.Controls
{
    public partial class QrCodeView : UserControl
    {
        private string? _barcode;

        public string Barcode
        {
            get => _barcode ?? string.Empty;
            set => Content = CreateImage(_barcode = value);
        }

        private static Border CreateImage(string barcode)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE
            };

            writer.Options.Width = 720;
            writer.Options.Height = 720;
            writer.Options.PureBarcode = true;
            writer.Options.Margin = 0;

            var bitmap = writer.WriteAsBitmap(barcode);

            using var memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Bmp);
            memory.Position = 0;
            var bitmapimage = new BitmapImage();
            bitmapimage.SetSource(memory.AsRandomAccessStream());
            var image = new Image
            {
                Source = bitmapimage,
                Margin = new Thickness(32),
                MaxWidth = 240,
                MaxHeight = 240
            };
            memory.Flush();
            return new Border
            {
                Background = new SolidColorBrush(Colors.White),
                CornerRadius = new CornerRadius(16),
                Child = image
            };
        }
    }
}
