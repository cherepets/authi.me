using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Authi.App.Maui;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Handlers;
using Color = Microsoft.Maui.Graphics.Color;

namespace Authi
{
    public partial class MainActivity
    {
        public static class MauiHandlers
        {
            public static void Initialize()
            {
                AddButton();
                AddEntry();
                AddSwitch();
            }

            private static void AddButton()
            {
                ButtonHandler.Mapper.AppendToMapping(nameof(ButtonHandler), (handler, view) =>
                {
                    if (handler.PlatformView.Background is RippleDrawable ripple)
                    {
                        var rippleColor = AuthiApp.Current.GetResource<Color>("Semitransparent1").ToAndroid();
                        ripple.SetColor(ColorStateList.ValueOf(rippleColor));
                    }
                });
            }

            private static void AddEntry()
            {
                EntryHandler.Mapper.AppendToMapping(nameof(IView.Background), (handler, view) =>
                {
                    var desiredHorizontalPadding = 12;
                    var desiredVerticalPadding = 8;
                    var entryCornerRadius = 8f;
                    float[] EntryRoundRectShape = [entryCornerRadius, entryCornerRadius, entryCornerRadius, entryCornerRadius, entryCornerRadius, entryCornerRadius, entryCornerRadius, entryCornerRadius];

                    var roundRectShape = new RoundRectShape(EntryRoundRectShape, null, null);
                    var shape = new ShapeDrawable(roundRectShape);
                    shape.Paint.SetStyle(Paint.Style.Stroke);
                    OnUnfocus();
                    handler.PlatformView.Background = shape;

                    var density = handler.PlatformView.Context.Resources.DisplayMetrics.Density;
                    var desiredHorizontalPaddingPx = (int)(desiredHorizontalPadding * density);
                    var desiredVerticalPaddingPx = (int)(desiredVerticalPadding * density);
                    handler.PlatformView.SetPadding(desiredHorizontalPaddingPx, desiredVerticalPaddingPx, desiredHorizontalPaddingPx, desiredVerticalPaddingPx);

                    handler.PlatformView.TextCursorDrawable.SetTint(AuthiApp.Current.GetResource<Color>("Primary").ToAndroid());

                    handler.PlatformView.FocusChange += (sender, e) =>
                    {
                        if (e.HasFocus)
                        {
                            OnFocus();
                        }
                        else
                        {
                            OnUnfocus();
                        }
                        handler.PlatformView.Invalidate();
                    };

                    void OnFocus()
                    {
                        shape.Paint.Color = AuthiApp.Current.GetResource<Color>("Primary").ToAndroid();
                        shape.Paint.StrokeWidth = 8;
                    }

                    void OnUnfocus()
                    {
                        shape.Paint.Color = AuthiApp.Current.GetResource<Color>("OnSurface").ToAndroid();
                        shape.Paint.StrokeWidth = 2;
                    }
                });
            }

            private static void AddSwitch()
            {
                SwitchHandler.Mapper.AppendToMapping(nameof(ISwitch.IsOn), (handler, view) =>
                {
                    var trackHeightDp = 28;
                    var trackWidthDp = 48;
                    var thumbSizeDp = view.IsOn ? 20 : 16;
                    var outlineDp = 2;

                    var density = handler.PlatformView.Context.Resources.DisplayMetrics.Density;
                    var trackHeightPx = (int)(trackHeightDp * density);
                    var trackWidthPx = (int)(trackWidthDp * density);
                    var thumbSizePx = (int)(thumbSizeDp * density);
                    var outlinePx = (int)(outlineDp * density);

                    var trackDrawable = new GradientDrawable();
                    trackDrawable.SetShape(ShapeType.Rectangle);
                    trackDrawable.SetCornerRadius(trackHeightPx / 2f);

                    var trackColor = view.IsOn
                        ? AuthiApp.Current.GetResource<Color>("Primary").ToAndroid()
                        : AuthiApp.Current.GetResource<Color>("Semitransparent").ToAndroid();

                    trackDrawable.SetColor(trackColor);

                    if (!view.IsOn)
                    {
                        var outlineColor = AuthiApp.Current.GetResource<Color>("Primary").ToAndroid();
                        trackDrawable.SetStroke(outlinePx, outlineColor);
                    }

                    var innerThumbDrawable = new GradientDrawable();
                    innerThumbDrawable.SetShape(ShapeType.Oval);

                    var thumbColor = view.IsOn
                        ? AuthiApp.Current.GetResource<Color>("OnPrimary").ToAndroid()
                        : AuthiApp.Current.GetResource<Color>("Primary").ToAndroid();

                    innerThumbDrawable.SetColor(thumbColor);

                    var thumbLayerDrawable = new LayerDrawable([innerThumbDrawable]);

                    thumbLayerDrawable.SetLayerSize(0, thumbSizePx, thumbSizePx);

                    var insetHorizontal = (trackHeightPx - thumbSizePx) / 2;
                    var insetVertical = (trackHeightPx - thumbSizePx) / 2;

                    if (insetHorizontal < 0) insetHorizontal = 0;
                    if (insetVertical < 0) insetVertical = 0;

                    thumbLayerDrawable.SetLayerInset(0, insetHorizontal, insetVertical, insetHorizontal, insetVertical);

                    handler.PlatformView.TrackDrawable = trackDrawable;
                    handler.PlatformView.ThumbDrawable = thumbLayerDrawable;

                    handler.PlatformView.SetMinimumWidth(trackWidthPx);
                    handler.PlatformView.SetMinimumHeight(trackHeightPx);
                });
            }
        }
    }
}
