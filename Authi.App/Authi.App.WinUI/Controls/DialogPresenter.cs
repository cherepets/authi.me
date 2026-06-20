using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Authi.App.WinUI.Controls
{
    public partial class DialogPresenter : FrameworkElement
    {
        public static DialogPresenter Current => _current!;
        private static DialogPresenter? _current;

        private TaskCompletionSource? _hidingTcs;
        private ContentDialog? _currentDialog;

        public DialogPresenter()
        {
            _current = this;
        }

        public async Task ShowDialogAsync(string? title, object content, string? primaryButtonText = null, string? cancelButtonText = null, Action? onPrimary = null, Action? onCancel = null)
        {
            if (_currentDialog != null)
            {
                await HideDialogAsync();
            }
            _currentDialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = cancelButtonText,
                XamlRoot = XamlRoot
            };
            _currentDialog.Loaded += OnDialogLoaded;
            if (!string.IsNullOrEmpty(primaryButtonText))
            {
                var result = await _currentDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    onPrimary?.Invoke();
                }
                else
                {
                    onCancel?.Invoke();
                }
            }
            else
            {
                await _currentDialog.ShowAsync();
                onCancel?.Invoke();
            }
        }

        public Task HideDialogAsync()
        {
            if (_currentDialog == null)
            {
                return Task.CompletedTask;
            }

            _hidingTcs = new TaskCompletionSource();
            _currentDialog.Hide();
            return _hidingTcs.Task;
        }

        private void OnDialogLoaded(object sender, RoutedEventArgs e)
        {
            var dialog = (ContentDialog)sender;
            dialog.Loaded -= OnDialogLoaded;
            dialog.Unloaded += OnDialogUnloaded;
        }

        private void OnDialogUnloaded(object sender, RoutedEventArgs e)
        {
            var dialog = (ContentDialog)sender;
            dialog.Unloaded -= OnDialogUnloaded;

            _currentDialog = null;
            _hidingTcs?.SetResult();
            _hidingTcs = null;
        }
    }
}
