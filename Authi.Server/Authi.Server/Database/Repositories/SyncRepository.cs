using Authi.Server.Database.Models;
using Authi.Server.Services;
using System;
using System.Threading.Tasks;

namespace Authi.Server.Database.Repositories
{
    internal interface ISyncRepository : IRepository<Sync> { }

    internal class SyncRepository(DatabaseContext context) : ServiceBase, ISyncRepository
    {
        public Task CreateAsync(Sync sync)
        {
            return context.InsertAsync(sync);
        }

        public Task<Sync?> ReadAsync(Guid id)
        {
            return context.FindAsync<Sync>(id);
        }

        public Task UpdateAsync(Sync sync)
        {
            return context.UpdateAsync(sync);
        }

        public Task DeleteAsync(Sync sync)
        {
            return context.DeleteAsync(sync);
        }
    }
}
