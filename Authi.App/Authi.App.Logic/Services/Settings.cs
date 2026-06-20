using Authi.Common.Extensions;
using Authi.Common.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Authi.App.Logic.Services
{
    [Service]
    public interface ISettings
    {
        IAsyncValueSetting<Guid> ClientId { get; }
        IAsyncValueSetting<Guid> Version { get; }
        IAsyncSetting<byte[]> SyncPrivateKey { get; }
        IAsyncSetting<byte[]> SyncPublicKey { get; }
        IAsyncSetting<byte[]> DataKey { get; }
        IAsyncSetting<string> ServerUrl { get; }
    }

    internal class Settings : ServiceBase, ISettings
    {
        public IAsyncValueSetting<Guid> ClientId { get; } = new AsyncValueSetting<Guid>(_inMemoryStorage);
        public IAsyncValueSetting<Guid> Version { get; } = new AsyncValueSetting<Guid>(_inMemoryStorage);
        public IAsyncSetting<byte[]> SyncPrivateKey { get; } = new AsyncSetting<byte[]>(_inMemoryStorage);
        public IAsyncSetting<byte[]> SyncPublicKey { get; } = new AsyncSetting<byte[]>(_inMemoryStorage);
        public IAsyncSetting<byte[]> DataKey { get; } = new AsyncSetting<byte[]>(_inMemoryStorage);
        public IAsyncSetting<string> ServerUrl { get; } = new AsyncSetting<string>(_inMemoryStorage);

        private readonly static Dictionary<string, byte[]?> _inMemoryStorage = [];
    }

    public class AsyncSetting<T> : ServiceBase, IAsyncSetting<T> where T : class
    {
        private readonly Dictionary<string, byte[]?> _inMemoryStorage = [];
        private readonly string _key;

        internal AsyncSetting(Dictionary<string, byte[]?> inMemoryStorage, [CallerMemberName] string? key = null)
        {
            Debug.Assert(key != null);

            _inMemoryStorage = inMemoryStorage;
            _key = key;
        }

        public async ValueTask<T?> GetAsync()
        {
            if (!_inMemoryStorage.TryGetValue(_key, out byte[]? bytes))
            {
                var base64 = await Services.SecureStorage.GetAsync(_key);
                if (!string.IsNullOrEmpty(base64))
                {
                    bytes = base64.ToBase64Bytes();
                    _inMemoryStorage[_key] = bytes;
                }
            }
            if (bytes == null)
            {
                return null;
            }
            return Services.BinarySerializer.Deserialize<T>(bytes);
        }

        public async ValueTask SetAsync(T? value)
        {
            var serialized = Services.BinarySerializer.Serialize(value);
            _inMemoryStorage[_key] = serialized;
            var base64 = serialized?.ToBase64String() ?? string.Empty;
            await Services.SecureStorage.SetAsync(_key, base64);
        }
    }

    public class AsyncValueSetting<T> : ServiceBase, IAsyncValueSetting<T> where T : struct
    {
        private readonly Dictionary<string, byte[]?> _inMemoryStorage = [];
        private readonly string _key;

        internal AsyncValueSetting(Dictionary<string, byte[]?> inMemoryStorage, [CallerMemberName] string? key = null)
        {
            Debug.Assert(key != null);

            _inMemoryStorage = inMemoryStorage;
            _key = key;
        }

        public async ValueTask<T?> GetAsync()
        {
            if (!_inMemoryStorage.TryGetValue(_key, out byte[]? bytes))
            {
                var base64 = await Services.SecureStorage.GetAsync(_key);
                if (!string.IsNullOrEmpty(base64))
                {
                    bytes = base64.ToBase64Bytes();
                    _inMemoryStorage[_key] = bytes;
                }
            }
            if (bytes == null)
            {
                return null;
            }
            return Services.BinarySerializer.DeserializeValue<T>(bytes);
        }

        public async ValueTask SetAsync(T? value)
        {
            var serialized = Services.BinarySerializer.Serialize(value);
            _inMemoryStorage[_key] = serialized;
            var base64 = serialized?.ToBase64String();
            if (string.IsNullOrEmpty(base64))
            {
                Services.SecureStorage.Remove(_key);
            }
            else
            {
                await Services.SecureStorage.SetAsync(_key, base64);
            }
        }
    }

    public interface IAsyncSetting<T> where T : class
    {
        ValueTask<T?> GetAsync();
        ValueTask SetAsync(T? value);
    }

    public interface IAsyncValueSetting<T> where T : struct
    {
        ValueTask<T?> GetAsync();
        ValueTask SetAsync(T? value);
    }
}
