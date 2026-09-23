using Authi.App.Logic;
using Authi.App.Logic.Services;
using Authi.App.WinUI.Extensions;
using Authi.App.WinUI.UI;
using Authi.Common.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppLifecycle;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using WinUIEx;
using L10n = Authi.App.Logic.Localization;
using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;

namespace Authi.App.WinUI
{
    public partial class App
    {
        public static new App Current => (App)Application.Current;

        public WindowEx MainWindow => _mainWindow!;
        private WindowEx? _mainWindow;

        private readonly TaskCompletionSource _launchTcs;

        private long? _deactivatedTimestamp;

        public App()
        {
            ServiceLocator.Init(
                typeof(ServiceLocator).Assembly,    // Authi.Common
                typeof(Config).Assembly,            // Authi.App.Logic
                typeof(App).Assembly);              // Authi.App
            InitializeComponent();
            _launchTcs = new TaskCompletionSource();
            Program.Activated += OnAppActivated;
        }

        public T GetResource<T>(string key) => (T)GetResource(key);

        public object GetResource(string key) => Resources[key];

        public bool IsForeground
        {
            get
            {
                var foregroundWindowHandle = Win32.Interop.User32.GetForegroundWindow();
                var currentWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
                return foregroundWindowHandle == currentWindowHandle;
            }
        }

        public async void OnAppActivated(object? sender, AppActivationArguments args)
        {
            await _launchTcs.Task;
            if (args.Kind == ExtendedActivationKind.Protocol && args.Data is ProtocolActivatedEventArgs protocolArgs)
            {
                _mainWindow?.DispatcherQueue.TryEnqueue(() =>
                    ServiceProvider.Current.Get<IMessenger>().DeeplinkActivated.Publish(this, protocolArgs.Uri.AbsoluteUri));

            }
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (!await TryUnlockAsync())
            {
                Exit();
                return;
            }
            ShowMainWindow();
            _launchTcs.SetResult();
        }

        private static async Task<bool> TryUnlockAsync()
        {
            var isBiometricsEnabled = await ServiceProvider.Current.Get<ISettings>().IsBiometricsEnabled.GetAsync() ?? false;
            return !isBiometricsEnabled || await ServiceProvider.Current.Get<IBiometrics>().VerifyAsync();
        }

        private void ShowMainWindow(WindowEx? previous = null)
        {
            var window = new WindowEx
            {
                WindowContent = new MainPage(),
                Width = previous?.Width ?? 520,
                Height = previous?.Height ?? 640,
                MinWidth = 340,
                MinHeight = 340,
                ExtendsContentIntoTitleBar = true,
                SystemBackdrop = new MicaBackdrop(),
                Title = L10n.Generic.AppName
            }.WithIcon("ms-appx:///Assets/AppIcon.ico");
            _mainWindow = window;
            window.Activated += OnWindowActivated;
            window.Activate();
            if (previous != null)
            {
                window.AppWindow.Move(previous.AppWindow.Position);
            }
        }

        private async void OnWindowActivated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == WindowActivationState.Deactivated)
            {
                _deactivatedTimestamp = ServiceProvider.Current.Get<IClock>().Timestamp;
                return;
            }

            var deactivatedTimestamp = _deactivatedTimestamp;
            _deactivatedTimestamp = null;

            if (deactivatedTimestamp is long timestamp &&
                !ServiceProvider.Current.Get<IClock>().IsRecent(timestamp, Config.LockTimeoutMs))
            {
                var lockedWindow = _mainWindow!;
                lockedWindow.Activated -= OnWindowActivated;
                lockedWindow.Hide();
                if (!await TryUnlockAsync())
                {
                    Exit();
                    return;
                }
                ShowMainWindow(lockedWindow);
                lockedWindow.Close();
            }

            ServiceProvider.Current.Get<IMessenger>().SyncNow.Publish(this);
        }
    }
}
