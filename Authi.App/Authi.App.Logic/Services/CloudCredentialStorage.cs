using AotServiceProvider;
using Authi.App.Logic.Data;
using Authi.App.Logic.Exceptions;
using Authi.Common.Client;
using Authi.Common.Extensions;
using Authi.Common.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Authi.App.Logic.Services
{
    [Service]
    internal interface ICloudCredentialStorage : ICredentialStorage
    {
        Task<bool> IsConnectedAsync();
        Task CommitAsync();
    }

    public class CloudCredentialStorage : ServiceBase, ICloudCredentialStorage
    {
        private Dictionary<Guid, Credential> _credentials = [];
        private bool _hasChanges;

        public async Task<bool> IsConnectedAsync()
        {
            var clientId = await Services.Settings.ClientId.GetAsync();
            return clientId.HasValue;
        }

        public Task InsertAsync(Credential data)
        {
            Debug.Assert(!data.CloudId.HasValue);

            _hasChanges = true;
            var cloudId = Guid.NewGuid();
            data.CloudId = cloudId;
            _credentials.Add(cloudId, data);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Credential data)
        {
            if (data.CloudId.HasValue)
            {
                _hasChanges = true;
                _credentials[data.CloudId.Value] = data;
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Credential data)
        {
            if (data.CloudId.HasValue)
            {
                _hasChanges = true;
                _credentials.Remove(data.CloudId.Value);
            }
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyCollection<Credential>> GetAllAsync()
        {
            var clientId = await Services.Settings.ClientId.GetAsync();
            var version = await Services.Settings.Version.GetAsync();
            var dataEncryptionKey = await Services.Settings.DataKey.GetAsync();
            var syncPrivateKey = await Services.Settings.SyncPrivateKey.GetAsync();
            var syncPublicKey = await Services.Settings.SyncPublicKey.GetAsync();

            if (!clientId.HasValue || dataEncryptionKey == null || syncPrivateKey == null || syncPublicKey == null)
            {
                throw new MissingSettingsException();
            }

            var syncKeyPair = new X25519KeyPair(
                new X25519PrivateKey(syncPrivateKey),
                new X25519PublicKey(syncPublicKey));
            var dataKey = new AesKey(dataEncryptionKey);

            var result = await Services.ApiClient.ReadAsync(
                clientId.Value,
                version ?? Guid.NewGuid(),
                dataKey,
                syncKeyPair);

            if (result.HasChanges)
            {
                await Services.Settings.Version.SetAsync(result.Version);
                _credentials = result.Credentials.ToDictionary(
                    x => x.CloudId,
                    x => x.MapPropertiesTo<Credential>());
            }
            else if (_credentials.Count == 0)
            {
                var localCredentials = await Services.LocalCredentialStorage.GetAllAsync();
                _credentials = localCredentials
                    .Where(x => x.CloudId.HasValue)
                    .ToDictionary(
                        x => x.CloudId!.Value,
                        x => x.MapPropertiesTo<Credential>());
            }

            return _credentials.Values;
        }

        public async Task CommitAsync()
        {
            if (!_hasChanges)
            {
                return;
            }

            var clientId = await Services.Settings.ClientId.GetAsync();
            var dataEncryptionKey = await Services.Settings.DataKey.GetAsync();
            var syncPrivateKey = await Services.Settings.SyncPrivateKey.GetAsync();
            var syncPublicKey = await Services.Settings.SyncPublicKey.GetAsync();

            if (!clientId.HasValue || dataEncryptionKey == null || syncPrivateKey == null || syncPublicKey == null)
            {
                throw new MissingSettingsException();
            }

            var syncKeyPair = new X25519KeyPair(
                new X25519PrivateKey(syncPrivateKey),
                new X25519PublicKey(syncPublicKey));
            var dataKey = new AesKey(dataEncryptionKey);

            var credentials = _credentials.Values
                .Select(x => x.MapPropertiesTo<CredentialDto>())
                .ToList();

            var result = await Services.ApiClient.WriteAsync(credentials, clientId.Value, dataKey, syncKeyPair);
            await Services.Settings.Version.SetAsync(result.Version);

            _hasChanges = false;
        }
    }
}
