using Authi.App.Logic.ViewModels;
using Authi.App.Maui.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using L10n = Authi.App.Logic.Localization;

namespace Authi.App.Maui.UI;

public partial class CredentialEditorView : IAdaptiveView
{
    private ICredentialEditorViewModel? ViewModel => BindingContext as ICredentialEditorViewModel;

    private readonly IReadOnlyCollection<Label> _entryLabels;
    private readonly IDictionary<object, Label> _labelForEntry;

    public CredentialEditorView()
    {
        InitializeComponent();

        var bottom = QrScanButton.HeightRequest + QrScanButton.Margin.VerticalThickness;
        CredentialsEditorView.Padding = new Thickness(0, 0, 0, bottom);
        _entryLabels =
        [
            TitleLabel,
            SecretLabel,
            SubtitleLabel
        ];
        _labelForEntry = new Dictionary<object, Label>()
        {
            { TitleEntry, TitleLabel  },
            { SecretEntry, SecretLabel },
            { SubtitleEntry, SubtitleLabel }
        };
    }

    public void SetCompactSize(bool isCompact)
    {
        Header.SetCompactSize(isCompact);
        SaveButton.IsVisible = !isCompact;
        CompactSaveButton.IsVisible = isCompact;
        foreach (var label in _entryLabels)
        {
            label.SetDynamicResource(BackgroundProperty, isCompact
                ? "SurfaceBrush"
                : "Surface2Brush");
        }
    }

    private void OnBindingContextChanged(object sender, EventArgs e)
    {
        if (ViewModel != null)
        {
            Header.Title = ViewModel.PageTitle;
        }
    }

    private void OnClose(object sender, EventArgs e)
    {
        ViewModel?.Close();
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        ViewModel?.Save();
    }

    private void OnFocusedEntry(object sender, FocusEventArgs e) => SetLabelFocusedState(_labelForEntry[sender]);

    private void OnUnfocusedEntry(object sender, FocusEventArgs e) => SetLabelUnfocusedState(_labelForEntry[sender]);

    private static void SetLabelFocusedState(Label label)
    {
        label.SetDynamicResource(Label.TextColorProperty, "Primary");
    }

    private static void SetLabelUnfocusedState(Label label)
    {
        label.SetDynamicResource(Label.TextColorProperty, "OnSurface");
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

    private void OnQrCodeDetected(string code)
    {
        if (ViewModel == null)
        {
            return;
        }
        if (ViewModel.QrScanned(code))
        {
            DialogPresenter.Current.HideDialog();
        }
    }
}
