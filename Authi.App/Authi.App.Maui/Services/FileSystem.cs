using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Storage;
using System.IO;
using System.Threading.Tasks;
using IFileSystem = Authi.App.Logic.Services.IFileSystem;
using MauiFileSystem = Microsoft.Maui.Storage.FileSystem;

namespace Authi.App.Maui.Services
{
    internal class FileSystem : IFileSystem
    {
        public string AppDataDirectory => MauiFileSystem.AppDataDirectory;

        public async Task<Stream?> ReadFromPickerAsync()
        {
            var file = await FilePicker.Default.PickAsync();
            if (file == null)
            {
                return null;
            }
            return await file.OpenReadAsync();
        }

        public async Task<bool> WriteToPickerAsync(Stream stream, string? suggestedFileName = null)
        {
            var result = await FileSaver.Default.SaveAsync(
                fileName: suggestedFileName ?? string.Empty,
                stream: stream);
            return result.IsSuccessful;
        }
    }
}
