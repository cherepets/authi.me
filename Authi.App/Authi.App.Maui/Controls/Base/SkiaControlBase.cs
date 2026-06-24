using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System;
using System.Diagnostics;

namespace Authi.App.Maui.Controls
{
    public abstract class SkiaControlBase : ContentView
    {
        private readonly TimeSpan _refreshInterval = TimeSpan.FromMilliseconds(16);
        private readonly Stopwatch _stopwatch = new();

        private readonly SKCanvasView _canvas;

        private bool _isDisposed;
        private bool _isValid;

        public SkiaControlBase()
        {
            Content = _canvas = new SKCanvasView();
            _canvas.PaintSurface += OnPaintSurface;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        protected void Invalidate()
        {
            _isValid = false;
        }

        protected abstract void Paint(SKPaintSurfaceEventArgs e, TimeSpan elapsedTime);

        private void StartRendering()
        {
            Dispatcher.StartTimer(_refreshInterval, () =>
            {
                if (_isDisposed)
                {
                    return false;
                }
                if (!_isValid)
                {
                    _canvas.InvalidateSurface();
                    _isValid = true;
                }
                else
                {
                    _stopwatch.Restart();
                }
                return true;
            });
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            var elapsed = _stopwatch.Elapsed;
            _stopwatch.Restart();

            Paint(e, elapsed);
        }

        private void OnLoaded(object? sender, EventArgs e)
        {
            Loaded -= OnLoaded;
            StartRendering();
        }

        private void OnUnloaded(object? sender, EventArgs e)
        {
            Unloaded -= OnUnloaded;
            _isDisposed = true;
        }
    }
}