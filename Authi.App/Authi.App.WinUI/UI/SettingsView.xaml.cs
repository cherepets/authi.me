using Authi.App.Logic.ViewModels;
using Authi.App.WinUI.Controls;
using Authi.Common.Extensions;
using Microsoft.UI.Xaml;
using L10n = Authi.App.Logic.Localization;

namespace Authi.App.WinUI.UI
{
    public sealed partial class SettingsView : IAdaptiveView
    {
        public SettingsViewModel? ViewModel { get; set; }

        public SettingsView()
        {
            InitializeComponent();
        }

        public void SetCompactSize(bool isCompact)
        {
            Header.SetCompactSize(isCompact);
        }

        private void OnUISyncEnabledSwitchToggled(object sender, RoutedEventArgs e)
        {
            ViewModel?.UISyncToggled(UISyncEnabledSwitch.IsOn);
        }

        private void OnCloseRequested(object sender, RoutedEventArgs e)
        {
            ViewModel?.Close();
        }

        private async void OnDownload()
        {
            ViewModel?.Download();
        }

        private async void OnUpload()
        {
            if (ViewModel == null) return;

            await DialogPresenter.Current.ShowDialogAsync(
                title: L10n.Settings.HostingSettingsTitle,
                content: new HostingSettingsView
                {
                    ViewModel = ViewModel
                },
                L10n.Generic.Confirm,
                L10n.Generic.Cancel,
                () => ViewModel?.Upload(),
                () =>
                {
                    if (ViewModel == null) return;
                    ViewModel.SelectedSyncServer = SettingsViewModel.SyncServerOption.AuthiCloud;
                });
        }

        private async void OnShowQR()
        {
            if (ViewModel == null) return;

            var bytes = await ViewModel.GetSyncCodeAsync();
            if (bytes != null)
            {
                var base64 = bytes.ToBase64String();
                var qrCode = new QrCodeView
                {
                    Barcode = base64
                };
                await DialogPresenter.Current.ShowDialogAsync(
                    title: null,
                    content: qrCode,
                    L10n.Generic.Cancel);
            }
        }

        private async void OnScanQR()
        {
            if (ViewModel == null) return;

            var scanner = new QrScanner();
            scanner.CodeDetected += OnQrCodeDetected;
            await DialogPresenter.Current.ShowDialogAsync(
                title: null,
                content: scanner,
                L10n.Generic.Cancel);
            scanner.CodeDetected -= OnQrCodeDetected;
        }

        private void OnQrCodeDetected(string code)
        {
            if (ViewModel == null) return;

            ViewModel.QrScanned(code.ToBase64Bytes());
        }
    }
}
