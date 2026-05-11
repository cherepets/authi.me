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

        public App(AppActivationArguments args)
        {
            ServiceLocator.Init(
                typeof(ServiceLocator).Assembly,    // Authi.Common
                typeof(Config).Assembly,            // Authi.App.Logic
                typeof(App).Assembly);              // Authi.App
            InitializeComponent();
            _launchTcs = new TaskCompletionSource();
            OnAppActivated(null, args);
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

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _mainWindow = new WindowEx
            {
                WindowContent = new MainPage(),
                Width = 520,
                Height = 640,
                MinWidth = 340,
                MinHeight = 340,
                ExtendsContentIntoTitleBar = true,
                SystemBackdrop = new MicaBackdrop(),
                Title = L10n.Generic.AppName
            }.WithIcon("ms-appx:///Assets/AppIcon.ico");
            _mainWindow.Activated += OnWindowActivated;
            _mainWindow.Activate();
            _launchTcs.SetResult();
        }

        private void OnWindowActivated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState != WindowActivationState.Deactivated)
            {
                ServiceProvider.Current.Get<IMessenger>().SyncNow.Publish(this);
            }
        }
    }
}
