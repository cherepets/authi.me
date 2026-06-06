using Authi.Common.Services;

namespace Authi.Server.Database
{
    [Service]
    internal interface IDatabase
    {
        IDatabaseScope CreateScope();
    }

    internal class Database : IDatabase
    {
        public IDatabaseScope CreateScope()
        {
            return new DatabaseScope();
        }
    }
}
