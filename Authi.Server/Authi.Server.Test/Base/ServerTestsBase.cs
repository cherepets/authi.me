using Authi.Common.Services;
using Authi.Common.Test;
using Authi.Server.Database;
using Authi.Server.Services;
using Authi.Server.Test.Mocks;

namespace Authi.Server.Test
{
    public class ServerTestsBase : TestsBase, IServiceConsumer
    {
        protected MockClientRepository ClientRepository { get; } = new();
        protected MockDataRepository DataRepository { get; } = new();
        protected MockSyncRepository SyncRepository { get; } = new();

        protected override void InitServiceLocator()
        {
            ServiceLocator.Init(
                typeof(ServiceLocator).Assembly,    // Authi.Common
                typeof(Program).Assembly);          // Authi.Server

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
        IServiceProvider IServiceConsumer.ServiceProvider => ServiceProvider.Current;
        #endregion
    }
}
