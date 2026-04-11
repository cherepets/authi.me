using Camera.MAUI;
using Camera.MAUI.ZXing;
using Camera.MAUI.ZXingHelper;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Authi.App.Maui.Controls;

public partial class QrScanner
{
    protected CameraView CameraView => CameraPlaceholder.Content as CameraView;

    public event Action<string> CodeDetected;

    private bool _isLoaded;

    public QrScanner()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, EventArgs e)
    {
        _isLoaded = true;
        await OnStateChanged(null);
    }

    private async void OnUnloaded(object sender, EventArgs e)
    {
        _isLoaded = false;
        await OnStateChanged(null);
    }

    private void OnFlash(object sender, EventArgs e)
    {
        var cameraView = CameraView;
        if (cameraView == null)
        {
            return;
        }
        cameraView.TorchEnabled = !cameraView.TorchEnabled;
    }

    private async void OnSwitchCamera(object sender, EventArgs e)
    {
        var cameraView = CameraView;
        if (cameraView == null)
        {
            return;
        }
        var currentIndex = cameraView.Cameras.IndexOf(cameraView.Camera);
        currentIndex++;
        if (currentIndex < cameraView.Cameras.Count)
        {
            cameraView.Camera = cameraView.Cameras[currentIndex];
        }
        else
        {
            cameraView.Camera = cameraView.Cameras.FirstOrDefault();
        }
        if (CameraPlaceholder.Width > CameraPlaceholder.Height)
        {
            await CameraPlaceholder.RotateYToAsync(90, AnimationLength.DefaultUnsigned, Easing.CubicIn);
            await cameraView.StopCameraAsync();
            CameraPlaceholder.RotationY = -90;
            await cameraView.StartCameraAsync();
            await Task.Delay(AnimationLength.Short);
            await CameraPlaceholder.RotateYToAsync(0, AnimationLength.DefaultUnsigned, Easing.CubicOut);
        }
        else
        {
            await CameraPlaceholder.RotateXToAsync(90, AnimationLength.DefaultUnsigned, Easing.CubicIn);
            await cameraView.StopCameraAsync();
            CameraPlaceholder.RotationX = -90;
            await cameraView.StartCameraAsync();
            await Task.Delay(AnimationLength.Short);
            await CameraPlaceholder.RotateXToAsync(0, AnimationLength.DefaultUnsigned, Easing.CubicOut);
        }
    }

    private void OnBarcodeDetected(object sender, BarcodeEventArgs args)
    {
        foreach (var barcode in args.Result)
        {
            MainThread.BeginInvokeOnMainThread(() => CodeDetected?.Invoke(barcode.Text));
        }
    }

    private async Task OnStateChanged(bool? visibilityChange)
    {
        if (visibilityChange == true)
        {
            TranslationY = 64;
            _ = this.TranslateToAsync(0, 0, AnimationLength.ShortUnsigned, Easing.CubicOut);
            await this.FadeToAsync(1, AnimationLength.ShortUnsigned, Easing.CubicOut);
        }
        else if (visibilityChange == false)
        {
            await Task.Delay(AnimationLength.Short);
        }

        var showCamera = _isLoaded;
        if (showCamera && CameraView == null)
        {
            var cameraView = new CameraView
            {
                FlashMode = FlashMode.Disabled,
                BarCodeDecoder = new ZXingBarcodeDecoder(),
                BarCodeOptions = new BarcodeDecodeOptions
                {
                    AutoRotate = true,
                    PossibleFormats = { BarcodeFormat.QR_CODE },
                    ReadMultipleCodes = false,
                    TryHarder = true,
                    TryInverted = true
                },
                BarCodeDetectionFrameRate = 10,
                BarCodeDetectionMaxThreads = 5,
                BarCodeDetectionEnabled = true,
                ControlBarcodeResultDuplicate = true,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };
            cameraView.CamerasLoaded += OnCamerasLoaded;
            cameraView.BarcodeDetected += OnBarcodeDetected;
            CameraPlaceholder.Content = cameraView;
        }
        else
        {
            if (CameraView is CameraView cameraView &&
                await cameraView.StopCameraAsync() == CameraResult.Success)
            {
                cameraView.CamerasLoaded -= OnCamerasLoaded;
                cameraView.BarcodeDetected -= OnBarcodeDetected;
                CameraPlaceholder.Content = null;
            }
        }
    }

    private void OnCamerasLoaded(object sender, EventArgs e)
    {
        var cameraView = CameraView;
        if (cameraView == null)
        {
            return;
        }
        if (cameraView.NumCamerasDetected > 0)
        {
            cameraView.Camera = cameraView.Cameras.OrderBy(c => c.Position).First();
            MainThread.BeginInvokeOnMainThread(() => cameraView.StartCameraAsync());
        }
    }
}