using Authi.Server.Database;
using Authi.Server.Database.Repositories;
using System.Threading.Tasks;

namespace Authi.Server.Test.Mocks
{
    internal class MockDatabaseScope(IClientRepository clientRepository, IDataRepository dataRepository, ISyncRepository syncRepository) : IDatabaseScope
    {
        public IClientRepository Client => clientRepository;
        public IDataRepository Data => dataRepository;
        public ISyncRepository Sync => syncRepository;

        public Task CleanUpAsync()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
