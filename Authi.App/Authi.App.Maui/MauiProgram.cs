using Camera.MAUI;
using CommunityToolkit.Maui;
using MaterialColorUtilities.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System;

namespace Authi.App.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            AppContext.SetSwitch("System.Reflection.NullabilityInfoContext.IsSupported", true);
            return MauiApp.CreateBuilder()
                .UseMaterialColors()
                .UseMauiApp<AuthiApp>()
                .UseMauiCommunityToolkit()
                .UseSkiaSharp()
                .UseMauiCameraView()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("MajorMonoDisplay.ttf", "MajorMonoDisplay");
                    fonts.AddFont("MaterialSymbols.ttf", "IconFont");
                    fonts.AddFont("NotoSans.ttf", "DefaultFont");
                })
                .Build();
        }
    }
}
