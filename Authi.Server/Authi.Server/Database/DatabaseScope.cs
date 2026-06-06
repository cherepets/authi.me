using Authi.Server.Database.Repositories;
using System;
using System.Threading.Tasks;

namespace Authi.Server.Database
{
    internal interface IDatabaseScope : IDisposable
    {
        IClientRepository Client { get; }
        IDataRepository Data { get; }
        ISyncRepository Sync { get; }

        Task CleanUpAsync();
    }

    internal class DatabaseScope : IDatabaseScope
    {
        private readonly DatabaseContext _context;

        public IClientRepository Client { get; }
        public IDataRepository Data { get; }
        public ISyncRepository Sync { get; }

        public DatabaseScope()
        {
            _context = new DatabaseContext();

            Client = new ClientRepository(_context);
            Data = new DataRepository(_context);
            Sync = new SyncRepository(_context);
        }

        public Task CleanUpAsync()
        {
            return _context.CleanUpAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
