using Authi.App.Logic;
using Authi.App.Logic.Services;
using Authi.App.Maui.Converters;
using Authi.App.Maui.Extensions;
using Authi.App.Maui.Styles;
using Authi.App.Maui.UI;
using Authi.Common.Services;
using MaterialColorUtilities.Maui;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace Authi.App.Maui
{
    public interface IAuthiApp
    {
        bool IsDarkMode { get; }
        T GetResource<T>(string key);
        object GetResource(string key);
        T GetThemedResource<T>(string key);
        object GetThemedResource(string key);
        void HandleDeeplink(string deeplink);
        void OpenMainPage();
        Task<bool> TryUnlockAsync();
    }

    public partial class AuthiApp : Application, IAuthiApp
    {
        public static new IAuthiApp Current => (AuthiApp)Application.Current!;

        public bool IsDarkMode => UserAppTheme == AppTheme.Dark;

        public Thickness SystemInsets { get; set; }

        private Shell? _shell;
        private bool _isUnlocked;
        private long? _deactivatedTimestamp;

        public AuthiApp()
        {
            UserAppTheme = RequestedTheme == AppTheme.Dark ? AppTheme.Dark : AppTheme.Light;

            ServiceLocator.Init(
                typeof(ServiceLocator).Assembly,    // Authi.Common
                typeof(Config).Assembly,            // Authi.App.Logic
                typeof(AuthiApp).Assembly);         // Authi.App

            Resources
                // Colors
                .Merge<XamarinColors>()
                .Merge<CustomColors>()
                .Merge<MaterialColorResourceDictionary>()
                // Styles
                .Merge<XamarinStyles>()
                .Merge<MaterialIcons>()
                .Merge<ButtonStyles>()
                .Merge<LabelStyles>()
                .Merge<SwitchStyles>()
                // Converters
                .Merge<ConvertersDictionary>();
        }

        public T GetResource<T>(string key) => (T)GetResource(key);

        public object GetResource(string key) => Resources[key];

        public T GetThemedResource<T>(string key) => (T)GetThemedResource(key);

        public object GetThemedResource(string key) => GetResource(key + (IsDarkMode ? "Dark" : "Light"));

        public void OpenMainPage()
        {
            if (_shell == null)
            {
                return;
            }

            _shell.Items.Clear();
            _shell.Items.Add(new MainPage());
        }

        public async Task<bool> TryUnlockAsync()
        {
            if (_isUnlocked)
            {
                return true;
            }

            var isBiometricsEnabled = await ServiceProvider.Current.Get<ISettings>().IsBiometricsEnabled.GetAsync() ?? false;
            _isUnlocked = !isBiometricsEnabled || await ServiceProvider.Current.Get<IBiometrics>().VerifyAsync();
            return _isUnlocked;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var shell = new Shell { FlyoutBehavior = FlyoutBehavior.Disabled };
            shell.SetDynamicResource(VisualElement.BackgroundProperty, "SurfaceBrush");
            var window = new Window(shell);
            _shell = shell;
            OpenMainPage();
            return window;
        }

        protected override void OnSleep()
        {
            _deactivatedTimestamp = ServiceProvider.Current.Get<IClock>().Timestamp;
        }

        protected override void OnResume()
        {
            var deactivatedTimestamp = _deactivatedTimestamp;
            _deactivatedTimestamp = null;

            if (_isUnlocked && deactivatedTimestamp is long timestamp &&
                !ServiceProvider.Current.Get<IClock>().IsRecent(timestamp, Config.LockTimeoutMs))
            {
                _isUnlocked = false;
                (_shell?.CurrentPage as MainPage)?.ViewModel.Dispose();
                OpenMainPage();
                return;
            }

            ServiceProvider.Current.Get<IMessenger>().SyncNow.Publish(this);
        }

        public void HandleDeeplink(string deeplink)
        {
            ServiceProvider.Current.Get<IMessenger>().DeeplinkActivated.Publish(this, deeplink);
        }
    }
}
