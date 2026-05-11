using Microsoft.UI.Dispatching;
using SharpHook;
using SharpHook.Data;

namespace Authi.App.WinUI.UI
{
    public partial class MainPage
    {
        private SimpleGlobalHook? _hook;
        private bool _isMetaPressed;

        private void ShortcutsEnable()
        {
            _hook = new SimpleGlobalHook();
            _hook.MousePressed += OnMousePressed;
            _hook.KeyPressed += OnKeyPressed;
            _hook.KeyReleased += OnKeyReleased;
            _hook.RunAsync();
        }

        private void ShortcutsDisable()
        {
            if (_hook != null)
            {
                _hook.MousePressed -= OnMousePressed;
                _hook.KeyPressed -= OnKeyPressed;
                _hook.KeyReleased -= OnKeyReleased;
                _hook.Dispose();
                _hook = null;
            }
        }

        private void OnMousePressed(object? sender, MouseHookEventArgs e)
        {
            if (e.Data.Button == MouseButton.Button4)
            {
                DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, OnBackButtonPressed);
            }
        }

        private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            if (e.Data.KeyCode == KeyCode.VcLeftMeta || e.Data.KeyCode == KeyCode.VcRightMeta)
            {
                _isMetaPressed = true;
            }

            if (e.Data.KeyCode == KeyCode.VcBackspace && _isMetaPressed)
            {
                DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, OnBackButtonPressed);
            }
        }

        private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
        {
            if (e.Data.KeyCode == KeyCode.VcLeftMeta || e.Data.KeyCode == KeyCode.VcRightMeta)
            {
                _isMetaPressed = false;
            }
        }
    }
}
