using Authi.App.Logic;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System;

namespace Authi.App.Maui.Controls
{
    public class ProgressBar : SkiaControlBase
    {
        private readonly SKColor _primaryColor;
        private readonly SKColor _secondaryColor;

        public ProgressBar()
        {
            _primaryColor = AuthiApp.Current.GetResource<Color>("Tertiary").ToSKColor();
            _secondaryColor = AuthiApp.Current.GetResource<Color>("OnTertiary").ToSKColor();
        }

        protected override void Paint(SKPaintSurfaceEventArgs e, TimeSpan elapsedTime)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            var timeLeft = (int)DateTime.Now.TimeOfDay.TotalMilliseconds % Config.UpdateMs;
            var progress = timeLeft / (float)Config.UpdateMs;
            var estimate = 1f - progress;

            canvas.Clear(SKColors.Transparent);

            var rectBottom = info.Height;
            var radius = rectBottom / 2;
            var diameter = rectBottom;

            var fullWidth = info.Width + radius * 4;
            var primaryLeft = -radius;
            var primaryRight = fullWidth * progress - radius * 4;

            var secondaryLeft = primaryRight + diameter;
            var secondaryRight = info.Width + radius;
            if (secondaryLeft > info.Width - diameter * 2)
            {
                secondaryLeft = info.Width - diameter * 2;
            }
            var circleRight = secondaryLeft + diameter;

            var primaryColor = _primaryColor;
            var secondaryColor = _secondaryColor;

            secondaryColor = new SKColor(
                (byte)(primaryColor.Red * progress + secondaryColor.Red * estimate),
                (byte)(primaryColor.Green * progress + secondaryColor.Green * estimate),
                (byte)(primaryColor.Blue * progress + secondaryColor.Blue * estimate),
                (byte)(primaryColor.Alpha * progress + secondaryColor.Alpha * estimate));

            using var primaryPaint = new SKPaint { Color = primaryColor, IsAntialias = true };
            using var secondaryPaint = new SKPaint { Color = secondaryColor, IsAntialias = true };

            var secondaryRect = new SKRoundRect(new SKRect(secondaryLeft, 0, secondaryRight, rectBottom), radius, radius);
            canvas.DrawRoundRect(secondaryRect, secondaryPaint);

            var primaryRect = new SKRoundRect(new SKRect(primaryLeft, 0, primaryRight, rectBottom), radius, radius);
            canvas.DrawRoundRect(primaryRect, primaryPaint);

            var circle = new SKRoundRect(new SKRect(secondaryLeft, 0, circleRight, rectBottom), radius, radius);
            canvas.DrawRoundRect(circle, primaryPaint);

            Invalidate();
        }
    }
}