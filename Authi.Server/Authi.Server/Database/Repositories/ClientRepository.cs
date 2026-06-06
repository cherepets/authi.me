using Authi.Server.Database.Models;
using Authi.Server.Services;
using System;
using System.Threading.Tasks;

namespace Authi.Server.Database.Repositories
{
    internal interface IClientRepository : IRepository<Client> { }

    internal class ClientRepository(DatabaseContext context) : ServiceBase, IClientRepository
    {
        public Task CreateAsync(Client client)
        {
            return context.InsertAsync(client);
        }

        public Task<Client?> ReadAsync(Guid id)
        {
            return context.FindAsync<Client>(id);
        }

        public Task UpdateAsync(Client client)
        {
            return context.UpdateAsync(client);
        }

        public Task DeleteAsync(Client client)
        {
            return context.DeleteAsync(client);
        }
    }
}
