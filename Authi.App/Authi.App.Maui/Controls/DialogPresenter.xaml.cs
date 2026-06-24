using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace Authi.App.Maui.Controls;

public partial class DialogPresenter
{
    public static DialogPresenter Current => _current!;
    private static DialogPresenter? _current;

    public bool IsPresenting
    {
        get => _isPresenting;
        private set
        {
            _isPresenting = value;
            IsPresentingChanged?.Invoke(this, value);
        }
    }

    public event EventHandler<bool>? IsPresentingChanged;

    private TaskCompletionSource? _hideDialogRequested;
    private Action? _primaryHandler;
    private Action? _cancelHandler;
    private bool _isPresenting;

    public DialogPresenter()
    {
        InitializeComponent();
        _current = this;
    }

    public async Task ShowDialogAsync(string? title, object content, string? primaryButtonText = null, string? cancelButtonText = null, Action? onPrimary = null, Action? onCancel = null)
    {
        TitleLabel.IsVisible = false;
        MessageLabel.IsVisible = false;
        ContentFrameGrid.IsVisible = false;
        PrimaryButton.IsVisible = false;
        CancelButton.IsVisible = false;

        _primaryHandler = onPrimary;
        _cancelHandler = onCancel;

        if (!string.IsNullOrEmpty(title))
        {
            TitleLabel.IsVisible = true;
            TitleLabel.Text = title;
        }

        if (content is string message && !string.IsNullOrEmpty(message))
        {
            MessageLabel.IsVisible = true;
            MessageLabel.Text = message;
        }
        else if (content is IView view)
        {
            ContentFrameGrid.IsVisible = true;
            ContentFrameGrid.Children.Clear();
            ContentFrameGrid.Children.Add(view);
        }

        if (!string.IsNullOrEmpty(primaryButtonText))
        {
            PrimaryButton.IsVisible = true;
            PrimaryButton.Text = primaryButtonText;
        }

        if (!string.IsNullOrEmpty(cancelButtonText))
        {
            CancelButton.IsVisible = true;
            CancelButton.Text = cancelButtonText;
        }

        _hideDialogRequested = new TaskCompletionSource();
        IsVisible = true;
        IsPresenting = true;
        DialogContainer.Scale = 0;
        BackgroundBorder.Opacity = 0;
        _ = BackgroundBorder.FadeToAsync(1, AnimationLength.ShortUnsigned, Easing.CubicOut);
        await DialogContainer.ScaleToAsync(1, AnimationLength.ShortUnsigned, Easing.CubicOut);
        await _hideDialogRequested.Task;
        IsVisible = false;
        IsPresenting = false;
        ContentFrameGrid.Children.Clear();
    }

    public void HideDialog()
    {
        _cancelHandler?.Invoke();
        _hideDialogRequested?.SetResult();
    }

    private void OnBackgroundTapped(object sender, TappedEventArgs e)
    {
        HideDialog();
    }

    private void PrimaryClicked(object sender, EventArgs e)
    {
        _primaryHandler?.Invoke();
        _hideDialogRequested?.SetResult();
    }

    private void CancelClicked(object sender, EventArgs e)
    {
        _cancelHandler?.Invoke();
        _hideDialogRequested?.SetResult();
    }
}
