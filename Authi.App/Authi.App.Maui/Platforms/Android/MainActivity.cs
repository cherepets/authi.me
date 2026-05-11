using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;
using Authi.App.Maui;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using static Android.Content.Intent;

namespace Authi
{
    [Activity(
        Theme = "@style/Maui.SplashTheme", 
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    [IntentFilter([ActionView],
        Categories = new[] { CategoryDefault, CategoryBrowsable },
        DataScheme = "otpauth",
        DataHost = "*",
        DataPathPrefix = "/")]
    public partial class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            MauiHandlers.Initialize();

            if (!AuthiApp.Current.IsDarkMode)
            {
                Window.InsetsController?.SetSystemBarsAppearance(
                    (int)WindowInsetsControllerAppearance.LightStatusBars,
                    (int)WindowInsetsControllerAppearance.LightStatusBars);
            }

            WindowCompat.SetDecorFitsSystemWindows(Window, false);

            Platform.CurrentActivity.Window.AddFlags(WindowManagerFlags.LayoutNoLimits);
            Platform.CurrentActivity.Window.AddFlags(WindowManagerFlags.TranslucentStatus);

            if (Intent is Intent intent)
            {
                OnIntent(intent);
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            OnIntent(intent);
        }

        private static void OnIntent(Intent intent)
        {
            if (intent?.DataString is string deeplink)
            {
                AuthiApp.Current.HandleDeeplink(deeplink);
            }
        }
    }
}
