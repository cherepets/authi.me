using Authi.App.Logic.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authi.App.Test.Mocks
{
    internal class SecureStorage : ISecureStorage
    {
        private readonly Dictionary<string, string> _storage = [];

        public Task<string?> GetAsync(string key)
            => Task.FromResult(_storage.TryGetValue(key, out string? value) ? value : null);

        public void Remove(string key)
            => _storage.Remove(key);

        public Task SetAsync(string key, string value)
        {
            _storage[key] = value;
            return Task.CompletedTask;
        }
    }
}
