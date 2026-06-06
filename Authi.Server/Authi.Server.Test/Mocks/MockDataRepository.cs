using Authi.Server.Database.Models;
using Authi.Server.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authi.Server.Services
{
    public class MockDataRepository : IDataRepository
    {
        private readonly Dictionary<Guid, Data> _storage = [];

        public Task CreateAsync(Data data)
        {
            _storage.Add(data.DataId, data);
            return Task.CompletedTask;
        }

        public Task<Data?> ReadAsync(Guid id)
        {
            var data = _storage.TryGetValue(id, out var value)
                ? value
                : null;
            return Task.FromResult(data);
        }

        public Task UpdateAsync(Data data)
        {
            _storage[data.DataId] = data;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Data data)
        {
            _storage.Remove(data.DataId);
            return Task.CompletedTask;
        }

        public Dictionary<Guid, Data> AsDictionary()
        {
            return _storage;
        }
    }
}
