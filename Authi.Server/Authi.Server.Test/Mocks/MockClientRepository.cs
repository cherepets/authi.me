using Authi.Common.Services;
using Authi.Server.Database;
using Authi.Server.Database.Models;
using Authi.Server.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authi.Server.Services
{
    public class MockClientRepository : IClientRepository
    {
        private readonly Dictionary<Guid, Client> _storage = [];

        public Task CreateAsync(Client client)
        {
            if (ServiceProvider.Current.Get<IDatabase>().CreateScope().Data.ReadAsync(client.DataId) is null)
            {
                throw new Exception($"Data with id {client.DataId} not found.");
            }

            _storage.Add(client.ClientId, client);
            return Task.CompletedTask;
        }

        public Task<Client?> ReadAsync(Guid id)
        {
            var client = _storage.TryGetValue(id, out var value)
                ? value
                : null;
            return Task.FromResult(client);
        }

        public Task UpdateAsync(Client client)
        {
            _storage[client.ClientId] = client;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Client client)
        {
            _storage.Remove(client.ClientId);
            return Task.CompletedTask;
        }

        public Dictionary<Guid, Client> AsDictionary()
        {
            return _storage;
        }
    }
}
