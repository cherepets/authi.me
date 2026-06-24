using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Activity;
using Authi.App.Maui;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
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
        private OnBackPressedCallback? _backPressedCallback;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            MauiHandlers.Initialize();
            _backPressedCallback = new AppBackPressedCallback(this);
            OnBackPressedDispatcher.AddCallback(this, _backPressedCallback);

            if (Platform.CurrentActivity?.Window is Android.Views.Window window)
            {
                EdgeToEdge.Enable(this);

                if (OperatingSystem.IsAndroidVersionAtLeast(30) &&
                    !AuthiApp.Current.IsDarkMode)
                {
                    window.InsetsController?.SetSystemBarsAppearance(
                        (int)WindowInsetsControllerAppearance.LightStatusBars,
                        (int)WindowInsetsControllerAppearance.LightStatusBars);
                }
            }

            if (Intent is Intent intent)
            {
                OnIntent(intent);
            }
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);
            OnIntent(intent);
        }

        private static void OnIntent(Intent? intent)
        {
            if (intent?.DataString is string deeplink)
            {
                AuthiApp.Current.HandleDeeplink(deeplink);
            }
        }

        private sealed class AppBackPressedCallback(MainActivity activity) : OnBackPressedCallback(true)
        {
            private readonly MainActivity _activity = activity;

            public override void HandleOnBackPressed()
            {
                var application = Microsoft.Maui.Controls.Application.Current;
                var window = application?.Windows.Count > 0
                    ? application.Windows[0]
                    : null;
                if (window?.Page is Shell shell && 
                    shell.CurrentPage is Page page &&
                    page.SendBackButtonPressed() == true)
                {
                    return;
                }

                Enabled = false;
                try
                {
                    _activity.OnBackPressedDispatcher.OnBackPressed();
                }
                finally
                {
                    Enabled = true;
                }
            }
        }
    }
}
