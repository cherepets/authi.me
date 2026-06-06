using Authi.Common.Services;
using Authi.Server.Database;

namespace Authi.Server.Services
{
    internal interface IServiceConsumer
    {
        IServiceProvider ServiceProvider { get; }

        public IAppHealthMonitor AppHealthMonitor => ServiceProvider.Get<IAppHealthMonitor>();
        public IClock Clock => ServiceProvider.Get<IClock>();
        public ICrypto Crypto => ServiceProvider.Get<ICrypto>();
        public IDatabase Database => ServiceProvider.Get<IDatabase>();
    }
}
