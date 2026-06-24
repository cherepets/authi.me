using System.Threading.Tasks;
using IClipboard = Authi.App.Logic.Services.IClipboard;
using MauiClipboard = Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard;

namespace Authi.App.Maui.Services
{
    internal class Clipboard : IClipboard
    {
        public Task<string?> GetTextAsync()
            => MauiClipboard.GetTextAsync();

        public Task SetTextAsync(string? text)
            => MauiClipboard.SetTextAsync(text);
    }
}
