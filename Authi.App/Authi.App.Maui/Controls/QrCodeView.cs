using Camera.MAUI;
using Camera.MAUI.ZXing;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Authi.App.Maui.Controls
{
    public partial class QrCodeView : Border
    {
        private string? _barcode;

        public string Barcode
        {
            get => _barcode ?? string.Empty;
            set => Content = CreateImage(_barcode = value);
        }

        public QrCodeView()
        {
            BackgroundColor = Colors.White;
            WidthRequest = 240;
            HeightRequest = 240;
            StrokeThickness = 0;
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(16)
            };
        }

        private static BarcodeImage CreateImage(string barcode)
        {
            return new BarcodeImage
            {
                BarcodeEncoder = new ZXingBarcodeEncoder(),
                BarcodeFormat = BarcodeFormat.QR_CODE,
                BarcodeBackground = Colors.White,
                BarcodeForeground = Colors.Black,
                Aspect = Aspect.Fill,
                BarcodeMargin = 0,
                Barcode = barcode
            };
        }
    }
}
