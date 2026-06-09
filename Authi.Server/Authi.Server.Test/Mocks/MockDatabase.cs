using Authi.Server.Database;
using System.Threading.Tasks;

namespace Authi.Server.Test.Mocks
{
    internal class MockDatabase(IDatabaseScope scope) : IDatabase
    {
        public IDatabaseScope CreateScope() => scope;
        public Task<long> GetSizeAsync() =>
            Task.FromResult(0L);
        public Task<bool> CanConnectAsync(string connectionString) =>
            Task.FromResult(true);
    }
}
