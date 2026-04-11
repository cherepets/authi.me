using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;
using Authi.App.Maui;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;

namespace Authi
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
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
        }
    }
}
