using Authi.Common.Services;
using System.Threading.Tasks;

namespace Authi.App.Logic.Services
{
    [Service]
    public interface IBiometrics
    {
        Task<bool> VerifyAsync();
    }
}
