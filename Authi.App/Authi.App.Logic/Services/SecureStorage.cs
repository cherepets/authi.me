using Authi.Common.Services;
using System.Threading.Tasks;

namespace Authi.App.Logic.Services
{
    [Service]
    public interface ISecureStorage
    {
        Task<string?> GetAsync(string key);
        Task SetAsync(string key, string value);
        void Remove(string key);
    }
}
