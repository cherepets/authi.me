using Authi.Server.Database.Models;
using Authi.Server.Services;
using System;
using System.Threading.Tasks;

namespace Authi.Server.Database.Repositories
{
    internal interface IDataRepository : IRepository<Data> { }

    internal class DataRepository(DatabaseContext context) : ServiceBase, IDataRepository
    {
        public Task CreateAsync(Data data)
        {
            return context.InsertAsync(data);
        }

        public Task<Data?> ReadAsync(Guid id)
        {
            return context.FindAsync<Data>(id);
        }

        public Task UpdateAsync(Data data)
        {
            return context.UpdateAsync(data);
        }

        public Task DeleteAsync(Data data)
        {
            return context.DeleteAsync(data);
        }
    }
}
