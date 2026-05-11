using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Authi.App.WinUI
{
    public class Program
    {
        private const string InstanceKey = "Authi.App.WinUI.Instance";

        private static IntPtr _redirectEventHandle = IntPtr.Zero;

        [STAThread]
        public static int Main()
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();

            var activationArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (!DecideRedirection(activationArgs))
            {
                Application.Start(p =>
                {
                    SynchronizationContext.SetSynchronizationContext(
                        syncContext: new DispatcherQueueSynchronizationContext(
                            dispatcherQueue: DispatcherQueue.GetForCurrentThread()));
                    _ = new App(activationArgs);
                });
            }

            return 0;
        }

        private static bool DecideRedirection(AppActivationArguments args)
        {
            var instance = AppInstance.FindOrRegisterForKey(InstanceKey);
            if (instance.IsCurrent)
            {
                instance.Activated += (s, e) => App.Current?.OnAppActivated(s, e);
                return false;
            }
            else
            {
                RedirectActivationTo(args, instance);
                return true;
            }
        }

        private static void RedirectActivationTo(AppActivationArguments args, AppInstance keyInstance)
        {
            _redirectEventHandle = Win32.Interop.Kernel32.CreateEvent(IntPtr.Zero, true, false, null);
            Task.Run(() =>
            {
                keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
                Win32.Interop.Kernel32.SetEvent(_redirectEventHandle);
            });

            uint CWMO_DEFAULT = 0;
            uint INFINITE = 0xFFFFFFFF;
            _ = Win32.Interop.Ole32.CoWaitForMultipleObjects(
               CWMO_DEFAULT, INFINITE, 1,
               [_redirectEventHandle], out uint handleIndex);

            Win32.Interop.User32.SetForegroundWindow(
                Process.GetProcessById((int)keyInstance.ProcessId)
                    .MainWindowHandle);
        }
    }
}
