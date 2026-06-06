using Authi.Common.Dto;
using Authi.Common.Extensions;
using System.Threading.Tasks;

namespace Authi.Server.ApiVersions
{
    public partial class ApiV1 : ApiVersionBase
    {
        public async Task<OptionalResponse<DeleteResponse>> OnDelete(DeleteRequest request)
        {
            using var db = Services.Database.CreateScope();

            var client = await db.Client.ReadAsync(request.ClientId);
            if (client == null)
            {
                return new ErrorResponse<DeleteResponse>(ErrorMessages.CantFindClient);
            }

            var keyPair = client.KeyPair.ToX25519KeyPair();

            string requestJson;
            try
            {
                requestJson = Services.Crypto.Decrypt(request.Body, keyPair).ToUtfString();
            }
            catch
            {
                return new ErrorResponse<DeleteResponse>(ErrorMessages.CantDecryptPayload);
            }

            var requestPayload = requestJson.FromJson<DeleteRequest.Payload>();
            if (VerifyPayload<DeleteResponse>(requestPayload) is { } error)
            {
                return error;
            }

            var data = await db.Data.ReadAsync(client.DataId);
            if (data == null)
            {
                return new ErrorResponse<DeleteResponse>(ErrorMessages.CantFindData);
            }

            var responsePayload = new DeleteResponse.Payload
            {
                Timestamp = Services.Clock.Timestamp
            };
            var responseBody = Services.Crypto.Encrypt(
                responsePayload.ToJson().ToUtfBytes(),
                keyPair);

            await db.Client.DeleteAsync(client);
            await db.Data.DeleteAsync(data);
            return new DeleteResponse
            {
                Body = responseBody
            };
        }
    }
}
