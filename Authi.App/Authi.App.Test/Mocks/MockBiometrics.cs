using Authi.App.Logic.Services;
using System.Threading.Tasks;

namespace Authi.App.Test.Mocks
{
    internal class MockBiometrics : IBiometrics
    {
        public Task<bool> VerifyAsync()
        {
            return Task.FromResult(true);
        }
    }
}
