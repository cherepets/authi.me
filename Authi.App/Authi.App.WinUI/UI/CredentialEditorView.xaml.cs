using Authi.App.Logic.ViewModels;
using Authi.App.WinUI.Controls;
using Microsoft.UI.Xaml;
using L10n = Authi.App.Logic.Localization;

namespace Authi.App.WinUI.UI
{
    public sealed partial class CredentialEditorView : IAdaptiveView
    {
        public ICredentialEditorViewModel? ViewModel { get; set; }

        public CredentialEditorView()
        {
            InitializeComponent();
        }

        public void SetCompactSize(bool isCompact)
        {
            Header.SetCompactSize(isCompact);
        }

        private void OnCloseRequested(object sender, RoutedEventArgs e)
        {
            ViewModel?.Close();
        }

        private void OnSaveClicked(object sender, RoutedEventArgs e)
        {
            ViewModel?.Save();
        }

        private async void OnScanQR(object sender, RoutedEventArgs e)
        {
            var scanner = new QrScanner();
            scanner.CodeDetected += OnQrCodeDetected;
            await DialogPresenter.Current.ShowDialogAsync(
                title: null,
                content: scanner,
                L10n.Generic.Cancel);
            scanner.CodeDetected -= OnQrCodeDetected;
        }

        private async void OnQrCodeDetected(string code)
        {
            if (ViewModel == null)
            {
                return;
            }
            if (ViewModel.QrScanned(code))
            {
                await DialogPresenter.Current.HideDialogAsync();
            }
        }
    }
}
