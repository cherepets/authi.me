using AotServiceProvider;
using AotServiceProvider.Abstractions;
using Authi.Common.Services;
using Authi.Common.Test;
using Authi.Server;
using Authi.Server.Database;
using Authi.Server.Services;
using Authi.Server.Test.Mocks;

[assembly: GenerateServiceConfig(
    typeof(Clock),             // Authi.Common
    typeof(Program)            // Authi.Server
)]

namespace Authi.Server.Test
{
    public class ServerTestsBase : TestsBase, IServiceConsumer
    {
        protected MockClientRepository ClientRepository { get; } = new();
        protected MockDataRepository DataRepository { get; } = new();
        protected MockSyncRepository SyncRepository { get; } = new();

        protected override IConfig CreateServiceConfig() => new ServiceConfig();

        protected override void InitServices()
        {
            ServicesMock.Override<IDatabase>(
                new MockDatabase(
                    new MockDatabaseScope(
                        ClientRepository,
                        DataRepository,
                        SyncRepository)));
        }

        protected static (X25519KeyPair, X25519KeyPair) ExchangePublicKeys(X25519KeyPair clientKeyPair, X25519KeyPair serverKeyPair)
        {
            return (
                new X25519KeyPair(
                    clientKeyPair.Private,
                    serverKeyPair.Public),
                new X25519KeyPair(
                    serverKeyPair.Private,
                    clientKeyPair.Public));
        }

        #region IServiceConsumer
        internal virtual IServiceConsumer Services => this;
        IServices IServiceConsumer.ServiceProvider => AotServiceProvider.Services.Current;
        #endregion
    }
}
