using Authi.Common.Dto;
using Authi.Common.Extensions;
using Authi.Common.Services;
using Authi.Server.Database.Models;
using System;
using System.Threading.Tasks;

namespace Authi.Server.ApiVersions
{
    public partial class ApiV1 : ApiVersionBase
    {
        public async Task<OptionalResponse<InitResponse>> OnInit(InitRequest request)
        {
            using var db = Services.Database.CreateScope();

            AesKey aesKey;
            try
            {
                aesKey = new AesKey(request.ClientPublicKey);
            }
            catch
            {
                return new ErrorResponse<InitResponse>(ErrorMessages.CantParseClientPublicKey);
            }

            string requestJson;
            try
            {
                requestJson = Services.Crypto.Decrypt(request.Body, aesKey).ToUtfString();
            }
            catch
            {
                return new ErrorResponse<InitResponse>(ErrorMessages.CantDecryptPayload);
            }

            var requestPayload = requestJson.FromJson<InitRequest.Payload>();
            if (VerifyPayload<InitResponse>(requestPayload) is { } error)
            {
                return error;
            }

            var serverKey = Services.Crypto.GenerateX25519KeyPair();

            var clientId = Guid.NewGuid();
            var dataId = Guid.NewGuid();
            var version = Guid.NewGuid();
            var data = new Data
            {
                DataId = dataId,
                Version = version,
                Binary = [],
                LastAccessedAt = Services.Clock.Timestamp
            };

            var client = new Client
            {
                ClientId = clientId,
                DataId = dataId,
                KeyPair = new KeyPair
                {
                    Private = serverKey.Private.ToString(),
                    Public = aesKey.ToString(),
                }
            };

            try
            {
                await db.Data.CreateAsync(data);
            }
            catch
            {
                return new ErrorResponse<InitResponse>(ErrorMessages.CantCreateData);
            }

            try
            {
                await db.Client.CreateAsync(client);
            }
            catch
            {
                return new ErrorResponse<InitResponse>(ErrorMessages.CantCreateClient);
            }

            var responsePayload = new InitResponse.Payload
            {
                ClientId = clientId,
                Version = version,
                Timestamp = Services.Clock.Timestamp
            };
            var responseBody = Services.Crypto.Encrypt(
                responsePayload.ToJson().ToUtfBytes(),
                client.KeyPair.ToX25519KeyPair());
            return new InitResponse
            {
                ServerPublicKey = serverKey.Public,
                Body = responseBody
            };
        }
    }
}
