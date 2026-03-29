using Authi.Common.Services;
using Authi.Server.Models;
using System;
using System.Threading.Tasks;

namespace Authi.Server.Services
{
    [Service]
    internal interface ISyncRepository : IRepository<Sync> { }

    internal class SyncRepository : ServiceBase, ISyncRepository
    {
        public Task CreateAsync(Sync sync)
        {
            return Services.AppDbContext.InsertAsync(sync);
        }

        public Task<Sync?> ReadAsync(Guid id)
        {
            return Services.AppDbContext.FindAsync<Sync>(id);
        }

        public Task UpdateAsync(Sync sync)
        {
            return Services.AppDbContext.UpdateAsync(sync);
        }

        public Task DeleteAsync(Sync sync)
        {
            return Services.AppDbContext.DeleteAsync(sync);
        }
    }
}
