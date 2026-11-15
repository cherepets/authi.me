using AotServiceProvider;
using System.Threading.Tasks;

namespace Authi.App.Logic.Services
{
    [Service]
    public interface ILinkOpener
    {
        Task OpenUriAsync(string uri);
    }
}
