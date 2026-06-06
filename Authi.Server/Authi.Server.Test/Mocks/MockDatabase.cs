using Authi.Server.Database;

namespace Authi.Server.Test.Mocks
{
    internal class MockDatabase(IDatabaseScope scope) : IDatabase
    {
        public IDatabaseScope CreateScope() => scope;
    }
}
