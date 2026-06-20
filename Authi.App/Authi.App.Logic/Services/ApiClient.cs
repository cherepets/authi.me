using Authi.Common.Client;
using Authi.Common.Client.Results;
using Authi.Common.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Client = Authi.Common.Client.ApiClient;

namespace Authi.App.Logic.Services
{
    [Service]
    internal interface IApiClient
    {
        Task<ConsumeResult> ConsumeAsync(SyncCode syncCode);
        Task<InitResult> InitAsync();
        Task<PublishResult> PublishAsync(Guid clientId, X25519KeyPair syncKeyPair);
        Task<ReadResult> ReadAsync(Guid clientId, Guid version, AesKey dataKey, X25519KeyPair syncKeyPair);
        Task<WriteResult> WriteAsync(IReadOnlyCollection<CredentialDto> credentials, Guid clientId, AesKey dataKey, X25519KeyPair syncKeyPair);
    }

    internal class ApiClient : ServiceBase, IApiClient
    {
        private Client? _client;

        public Task<ConsumeResult> ConsumeAsync(SyncCode syncCode)
            => Execute(client 
                => client.ConsumeAsync(syncCode));

        public Task<InitResult> InitAsync()
            => Execute(client
                => client.InitAsync());

        public Task<PublishResult> PublishAsync(Guid clientId, X25519KeyPair syncKeyPair)
            => Execute(client
                => client.PublishAsync(clientId, syncKeyPair));

        public Task<ReadResult> ReadAsync(Guid clientId, Guid version, AesKey dataKey, X25519KeyPair syncKeyPair)
            => Execute(client
                => client.ReadAsync(clientId, version, dataKey, syncKeyPair));

        public Task<WriteResult> WriteAsync(IReadOnlyCollection<CredentialDto> credentials, Guid clientId, AesKey dataKey, X25519KeyPair syncKeyPair)
            => Execute(client
                => client.WriteAsync(credentials, clientId, dataKey, syncKeyPair));

        private async Task<T> Execute<T>(Func<Client, Task<T>> predicate)
        {
            var client = await GetClientAsync();
            return await predicate(client);
        }

        private async ValueTask<Client> GetClientAsync()
        {
            var serverUrl = await Services.Settings.ServerUrl.GetAsync() ?? Client.DefaultServerUrl;
            if (_client != null)
            {
                if (_client.ServerUrl == serverUrl)
                {
                    return _client;
                }
                _client.Dispose();
            }
            _client = new Client(serverUrl, Services.Clock, Services.Crypto);
            return _client;
        }
    }
}
