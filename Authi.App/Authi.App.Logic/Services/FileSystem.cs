using AotServiceProvider;
using System.IO;
using System.Threading.Tasks;

namespace Authi.App.Logic.Services
{
    [Service]
    public interface IFileSystem
    {
        string AppDataDirectory { get; }
        Task<Stream?> ReadFromPickerAsync();
        Task<bool> WriteToPickerAsync(Stream stream, string? suggestedFileName = null);
    }
}
