using Authi.Common.Services;
using Authi.Server.Models;
using System;
using System.Threading.Tasks;

namespace Authi.Server.Services
{
    [Service]
    internal interface IClientRepository : IRepository<Client> { }

    internal class ClientRepository : ServiceBase, IClientRepository
    {
        public Task CreateAsync(Client client)
        {
            return Services.AppDbContext.InsertAsync(client);
        }

        public Task<Client?> ReadAsync(Guid id)
        {
            return Services.AppDbContext.FindAsync<Client>(id);
        }

        public Task UpdateAsync(Client client)
        {
            return Services.AppDbContext.UpdateAsync(client);
        }

        public Task DeleteAsync(Client client)
        {
            return Services.AppDbContext.DeleteAsync(client);
        }
    }
}
