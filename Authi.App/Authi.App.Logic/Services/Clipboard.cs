using AotServiceProvider;
using System.Threading.Tasks;

namespace Authi.App.Logic.Services
{
    [Service]
    public interface IClipboard
    {
        Task SetTextAsync(string? text);
        Task<string?> GetTextAsync();
    }
}
