using Authi.Common.Services;
using Authi.Server.Models;
using System;
using System.Threading.Tasks;

namespace Authi.Server.Services
{
    [Service]
    internal interface IDataRepository : IRepository<Data> { }

    internal class DataRepository : ServiceBase, IDataRepository
    {
        public Task CreateAsync(Data data)
        {
            return Services.AppDbContext.InsertAsync(data);
        }

        public Task<Data?> ReadAsync(Guid id)
        {
            return Services.AppDbContext.FindAsync<Data>(id);
        }

        public Task UpdateAsync(Data data)
        {
            return Services.AppDbContext.UpdateAsync(data);
        }

        public Task DeleteAsync(Data data)
        {
            return Services.AppDbContext.DeleteAsync(data);
        }
    }
}
