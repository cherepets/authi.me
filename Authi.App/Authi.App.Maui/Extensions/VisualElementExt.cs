using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace Authi.App.Maui.Extensions
{
    public static class VisualElementExt
    {
        public static async Task FinishLayoutingAsync(this VisualElement element)
        {
            await Task.Delay(1);

            if (element.Width == -1 && element.Height == -1)
            {
                var tcs = new TaskCompletionSource<bool>();

                void FinishedLayouting(object? sender, EventArgs args)
                {
                    element.SizeChanged -= FinishedLayouting;
                    tcs.TrySetResult(true);
                }

                element.SizeChanged += FinishedLayouting;

                await tcs.Task;
            }
        }
    }
}
