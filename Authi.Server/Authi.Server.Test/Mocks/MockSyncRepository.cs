using Authi.Common.Services;
using Authi.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authi.Server.Services
{
    public class MockSyncRepository : ISyncRepository
    {
        private Dictionary<Guid, Sync> _storage = [];

        public Task CreateAsync(Sync sync)
        {
            if (ServiceProvider.Current.Get<IDataRepository>().ReadAsync(sync.DataId) is null)
            {
                throw new Exception($"Data with id {sync.DataId} not found.");
            }

            _storage.Add(sync.SyncId, sync);
            return Task.CompletedTask;
        }

        public Task<Sync?> ReadAsync(Guid id)
        {
            var sync = _storage.TryGetValue(id, out var value)
                ? value
                : null;
            return Task.FromResult(sync);
        }

        public Task UpdateAsync(Sync sync)
        {
            _storage[sync.SyncId] = sync;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Sync sync)
        {
            _storage.Remove(sync.SyncId);
            return Task.CompletedTask;
        }

        public void Initialize(params Sync[] records)
        {
            _storage = records.ToDictionary(x => x.SyncId);
        }

        public Dictionary<Guid, Sync> AsDictionary()
        {
            return _storage;
        }
    }
}
