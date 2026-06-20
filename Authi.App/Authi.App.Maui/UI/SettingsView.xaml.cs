using Authi.App.Logic.ViewModels;
using Authi.App.Maui.Controls;
using Authi.Common.Extensions;
using Microsoft.Maui.Controls;
using System;
using L10n = Authi.App.Logic.Localization;

namespace Authi.App.Maui.UI;

public partial class SettingsView : IAdaptiveView
{
    private SettingsViewModel ViewModel => BindingContext as SettingsViewModel;

    public SettingsView()
    {
        InitializeComponent();
    }

    public void SetCompactSize(bool isCompact)
    {
        Header.SetCompactSize(isCompact);
    }

    private void OnUISyncEnabledSwitchToggled(object sender, ToggledEventArgs e)
    {
        ViewModel?.UISyncToggled(e.Value);
    }

    private async void OnDownload(object sender, EventArgs e)
    {
        ViewModel?.Download();
    }

    private async void OnUpload(object sender, EventArgs e)
    {
        if (ViewModel == null) return;

        await DialogPresenter.Current.ShowDialogAsync(
            title: L10n.Settings.HostingSettingsTitle,
            content: new HostingSettingsView
            {
                BindingContext = ViewModel
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

    private async void OnShowQR(object sender, EventArgs e)
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

    private void OnCopySyncCode(object sender, EventArgs e)
    {
        ViewModel?.CopySyncCode();
    }

    private async void OnScanQR(object sender, EventArgs e)
    {
        var scanner = new QrScanner();
        scanner.CodeDetected += OnQrCodeDetected;
        await DialogPresenter.Current.ShowDialogAsync(
            title: null,
            content: scanner,
            L10n.Generic.Cancel);
        scanner.CodeDetected -= OnQrCodeDetected;
    }

    private void OnPasteSyncCode(object sender, EventArgs e)
    {
        ViewModel?.PasteSyncCode();
    }

    private void OnClose(object sender, EventArgs e)
    {
        ViewModel?.Close();
    }

    private void OnImport(object sender, EventArgs e)
    {
        ViewModel?.Import();
    }

    private void OnExport(object sender, EventArgs e)
    {
        ViewModel?.Export();
    }

    private void OnGetApp(object sender, EventArgs e)
    {
        ViewModel?.GetApp();
    }

    private void OnQrCodeDetected(string code)
    {
        if (ViewModel == null) return;

        ViewModel.QrScanned(code.ToBase64Bytes());
    }
}
