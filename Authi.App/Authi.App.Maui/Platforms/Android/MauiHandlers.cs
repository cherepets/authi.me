using Android.Content.Res;
using Android.Graphics.Drawables;
using Authi.App.Maui;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Handlers;
using System;
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
                AddRadioButton();
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
                    var density = handler.PlatformView.Context?.Resources?.DisplayMetrics?.Density;
                    if (!density.HasValue)
                    {
                        return;
                    }

                    var radiusPx = 8f * density.Value;

                    var focusedColor = AuthiApp.Current.GetResource<Color>("Primary").ToAndroid();
                    var focusedShape = new GradientDrawable();
                    focusedShape.SetShape(ShapeType.Rectangle);
                    focusedShape.SetCornerRadius(radiusPx);
                    focusedShape.SetColor(Android.Graphics.Color.Transparent);
                    focusedShape.SetStroke((int)(2.5f * density), focusedColor);

                    var unfocusedColor = AuthiApp.Current.GetResource<Color>("OnSurface").ToAndroid();
                    var unfocusedShape = new GradientDrawable();
                    unfocusedShape.SetShape(ShapeType.Rectangle);
                    unfocusedShape.SetCornerRadius(radiusPx);
                    unfocusedShape.SetColor(Android.Graphics.Color.Transparent);
                    unfocusedShape.SetStroke((int)(1f * density), unfocusedColor);

                    var stateListDrawable = new StateListDrawable();
                    stateListDrawable.AddState([Android.Resource.Attribute.StateFocused], focusedShape);
                    stateListDrawable.AddState([], unfocusedShape);

                    handler.PlatformView.Background = stateListDrawable;

                    var hPaddingPx = (int)(12 * density);
                    var vPaddingPx = (int)(8 * density);
                    handler.PlatformView.SetPadding(hPaddingPx, vPaddingPx, hPaddingPx, vPaddingPx);

                    if (OperatingSystem.IsAndroidVersionAtLeast(29))
                    {
                        handler.PlatformView.TextCursorDrawable?.SetTint(focusedColor);
                    }
                });
            }

            private static void AddSwitch()
            {
                SwitchHandler.Mapper.AppendToMapping(nameof(ISwitch.IsOn), (handler, view) =>
                {
                    var density = handler.PlatformView.Context?.Resources?.DisplayMetrics?.Density;
                    if (!density.HasValue)
                    {
                        return;
                    }

                    var trackHeightDp = 28;
                    var trackWidthDp = 48;
                    var thumbSizeDp = view.IsOn ? 20 : 16;
                    var outlineDp = 2;

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

            private static void AddRadioButton()
            {
                RadioButtonHandler.Mapper.AppendToMapping(nameof(RadioButtonHandler), (handler, view) =>
                {
                    if (handler.PlatformView is AndroidX.AppCompat.Widget.AppCompatRadioButton androidRadioButton)
                    {
                        androidRadioButton.ButtonTintList = new ColorStateList(
                            [
                                [Android.Resource.Attribute.StateChecked],
                                [-Android.Resource.Attribute.StateChecked]
                            ],
                            [
                                AuthiApp.Current.GetResource<Color>("Primary").ToAndroid(),
                                AuthiApp.Current.GetResource<Color>("Outline").ToAndroid()
                            ]
                        );
                    }
                });
            }
        }
    }
}
