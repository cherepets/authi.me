using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System.ComponentModel;

namespace Authi.App.Maui.Controls
{
    public class AniGrid : Grid
    {
        public enum Direction
        {
            Left, Top, Right, Bottom
        }

        public static new readonly BindableProperty IsVisibleProperty = BindableProperty.Create(nameof(IsVisible), typeof(bool), typeof(AniGrid), true, propertyChanged: OnIsVisibleChanged);

        [TypeConverter(typeof(VisibilityConverter))]
        public new bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public Direction FromDirection { get; set; }

        private static void OnIsVisibleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((AniGrid)bindable).OnIsVisibleChanged((bool)newValue);
        }

        private void OnIsVisibleChanged(bool newValue)
        {
            base.IsVisible = newValue;
            if (newValue)
            {
                switch (FromDirection)
                {
                    case Direction.Left:
                        TranslationX = -64;
                        break;
                    case Direction.Top:
                        TranslationY = -64;
                        break;
                    case Direction.Right:
                        TranslationX = 64;
                        break;
                    case Direction.Bottom:
                        TranslationY = 64;
                        break;
                }
                this.TranslateToAsync(0, 0, AnimationLength.ShortUnsigned, Easing.CubicOut);
                this.FadeToAsync(1, AnimationLength.ShortUnsigned, Easing.CubicOut);
            }
        }
    }
}
