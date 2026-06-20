using Authi.Common.Client.Exceptions;
using Authi.Common.Client.Results;
using Authi.Common.Dto;
using Authi.Common.Extensions;
using Authi.Common.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Authi.Common.Client
{
    public class ApiClient(string? serverUrl, IClock clock, ICrypto crypto) : IDisposable
    {
        public const string DefaultServerUrl = "authi.runasp.net";

        public string ServerUrl => serverUrl ?? DefaultServerUrl;

        private readonly TimeSpan ResponseValidFor = TimeSpan.FromSeconds(30);

        private readonly Api _api = new(serverUrl ?? DefaultServerUrl);

        public async Task<ConsumeResult> ConsumeAsync(SyncCode syncCode)
        {
            // Use temp instance of Api, not an existing one
            // to make sure ServerUrl is coming from syncCode
            using var api = new Api(syncCode.ServerUrl ?? DefaultServerUrl);

            var clientKeyPair = crypto.GenerateX25519KeyPair();
            var requestPayload = new ConsumeRequest.Payload
            {
                ClientPublicKey = clientKeyPair.Public,
                Timestamp = clock.Timestamp
            };
            var requestBody = crypto.Encrypt(
                requestPayload.ToJson().ToUtfBytes(),
                syncCode.OneTimeKey);
            var request = new ConsumeRequest
            {
                SyncId = syncCode.SyncId,
                Body = requestBody
            };

            var response = await api.ConsumeAsync(request);

            var responseJson = crypto.Decrypt(response.Body, syncCode.OneTimeKey).ToUtfString();
            var responsePayload = responseJson.FromJson<ConsumeResponse.Payload>();
            VerifyPayload(responsePayload);

            var serverPublicKey = new X25519PublicKey(responsePayload.ServerPublicKey);
            return new ConsumeResult
            {
                ClientId = responsePayload.ClientId,
                DataKey = syncCode.DataKey,
                SyncKeyPair = new X25519KeyPair(
                    clientKeyPair.Private,
                    serverPublicKey),
                ServerUrl = syncCode.ServerUrl
            };
        }

        public async Task<InitResult> InitAsync()
        {
            var clientKeyPair = crypto.GenerateX25519KeyPair();
            var requestPayload = new InitRequest.Payload
            {
                Timestamp = clock.Timestamp
            };
            var requestBody = crypto.Encrypt(
                requestPayload.ToJson().ToUtfBytes(),
                new AesKey(clientKeyPair.Public));
            var request = new InitRequest
            {
                ClientPublicKey = clientKeyPair.Public,
                Body = requestBody
            };

            var response = await _api.InitAsync(request);

            var syncKeyPair = new X25519KeyPair(
                new X25519PrivateKey(clientKeyPair.Private),
                new X25519PublicKey(response.ServerPublicKey));

            var responseJson = crypto.Decrypt(response.Body, syncKeyPair).ToUtfString();
            var responsePayload = responseJson.FromJson<InitResponse.Payload>();
            VerifyPayload(responsePayload);

            var serverPublicKey = new X25519PublicKey(response.ServerPublicKey);
            var dataKey = crypto.GenerateAesKey();
            return new InitResult
            {
                ClientId = responsePayload.ClientId,
                DataKey = dataKey,
                SyncKeyPair = new X25519KeyPair(
                    clientKeyPair.Private,
                    serverPublicKey)
            };
        }

        public async Task<PublishResult> PublishAsync(Guid clientId, X25519KeyPair syncKeyPair)
        {
            var oneTimeKey = crypto.GenerateX25519KeyPair();
            var requestPayload = new PublishRequest.Payload
            {
                OneTimeClientPublicKey = oneTimeKey.Public,
                Timestamp = clock.Timestamp
            };
            var requestBody = crypto.Encrypt(
                requestPayload.ToJson().ToUtfBytes(),
                syncKeyPair);
            var request = new PublishRequest
            {
                ClientId = clientId,
                Body = requestBody
            };

            var response = await _api.PublishAsync(request);

            var responseJson = crypto.Decrypt(response.Body, syncKeyPair).ToUtfString();
            var responsePayload = responseJson.FromJson<PublishResponse.Payload>();
            VerifyPayload(responsePayload);

            return new PublishResult
            {
                SyncId = responsePayload.SyncId,
                OneTimeKey = new X25519KeyPair(oneTimeKey.Private, new X25519PublicKey(responsePayload.ServerPublicKey)).DeriveAesKey()
            };
        }

        public async Task<ReadResult> ReadAsync(Guid clientId, Guid version, AesKey dataKey, X25519KeyPair syncKeyPair)
        {
            var requestPayload = new ReadRequest.Payload
            {
                Timestamp = clock.Timestamp,
                Version = version
            };
            var requestBody = crypto.Encrypt(
                requestPayload.ToJson().ToUtfBytes(),
                syncKeyPair);
            var request = new ReadRequest
            {
                ClientId = clientId,
                Body = requestBody
            };

            var response = await _api.ReadAsync(request);

            var responseJson = crypto.Decrypt(response.Body, syncKeyPair).ToUtfString();
            var responsePayload = responseJson.FromJson<ReadResponse.Payload>();
            VerifyPayload(responsePayload);

            var credentials = responsePayload.Binary.Length > 0
                ? crypto
                    .Decrypt(responsePayload.Binary, dataKey)
                    .ToUtfString()
                    .FromJson<CredentialDto[]>()
                : null;

            return new ReadResult
            {
                Credentials = credentials ?? [],
                Version = responsePayload.Version,
                HasChanges = responsePayload.HasChanges
            };
        }

        public async Task<WriteResult> WriteAsync(IReadOnlyCollection<CredentialDto> credentials, Guid clientId, AesKey dataKey, X25519KeyPair syncKeyPair)
        {
            var credentialsJson = credentials.ToJson();
            var dataBinary = credentialsJson.ToUtfBytes();
            var requestPayload = new WriteRequest.Payload
            {
                Binary = crypto.Encrypt(dataBinary, dataKey),
                Timestamp = clock.Timestamp
            };
            var requestBody = crypto.Encrypt(
                requestPayload.ToJson().ToUtfBytes(),
                syncKeyPair);
            var request = new WriteRequest
            {
                ClientId = clientId,
                Body = requestBody
            };
            var response = await _api.WriteAsync(request);

            var responseJson = crypto.Decrypt(response.Body, syncKeyPair).ToUtfString();
            var responsePayload = responseJson.FromJson<WriteResponse.Payload>();
            VerifyPayload(responsePayload);

            return new WriteResult
            {
                Version = responsePayload.Version
            };
        }

        private void VerifyPayload([NotNull] PayloadBase? payload)
        {
            if (payload == null)
            {
                payload = new PayloadBase { Timestamp = 0 };
                throw new ApiException("Can't parse payload.");
            }

            if (!clock.IsRecent(payload.Timestamp, ResponseValidFor))
            {
                throw new ApiException("Can't verify clock.");
            }
        }

        public void Dispose()
        {
            _api.Dispose();
        }
    }
}
