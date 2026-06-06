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
        public async Task<OptionalResponse<PublishResponse>> OnPublish(PublishRequest request)
        {
            using var db = Services.Database.CreateScope();

            var client = await db.Client.ReadAsync(request.ClientId);
            if (client == null)
            {
                return new ErrorResponse<PublishResponse>(ErrorMessages.CantFindClient);
            }

            var keyPair = client.KeyPair.ToX25519KeyPair();

            string requestJson;
            try
            {
                requestJson = Services.Crypto.Decrypt(request.Body, keyPair).ToUtfString();
            }
            catch
            {
                return new ErrorResponse<PublishResponse>(ErrorMessages.CantDecryptPayload);
            }

            var requestPayload = requestJson.FromJson<PublishRequest.Payload>();
            if (VerifyPayload<PublishResponse>(requestPayload) is { } error)
            {
                return error;
            }

            X25519PublicKey oneTimeClientPublicKey;
            try
            {
                oneTimeClientPublicKey = new X25519PublicKey(requestPayload.OneTimeClientPublicKey);
            }
            catch
            {
                return new ErrorResponse<PublishResponse>(ErrorMessages.CantParseClientPublicKey);
            }

            var oneTimeKeyPair = Services.Crypto.GenerateX25519KeyPair();

            var sync = new Sync
            {
                SyncId = Guid.NewGuid(),
                DataId = client.DataId,
                CreatedAt = Services.Clock.Timestamp,
                OneTimeKeyPair = new KeyPair
                {
                    Private = oneTimeKeyPair.Private.ToString(),
                    Public = oneTimeClientPublicKey.ToString()
                }
            };
            await db.Sync.CreateAsync(sync);

            var responsePayload = new PublishResponse.Payload
            {
                Timestamp = Services.Clock.Timestamp,
                SyncId = sync.SyncId,
                ServerPublicKey = oneTimeKeyPair.Public
            };
            var responseBody = Services.Crypto.Encrypt(
                responsePayload.ToJson().ToUtfBytes(),
                keyPair);

            return new PublishResponse
            {
                Body = responseBody
            };
        }
    }
}
