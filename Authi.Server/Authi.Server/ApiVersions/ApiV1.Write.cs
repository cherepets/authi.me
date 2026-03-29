using Authi.Common.Dto;
using Authi.Common.Extensions;
using System;
using System.Threading.Tasks;

namespace Authi.Server.ApiVersions
{
    public partial class ApiV1 : ApiVersionBase
    {
        private const int DataLimit = 100000;

        public async Task<OptionalResponse<WriteResponse>> OnWrite(WriteRequest request)
        {
            var client = await Services.ClientRepository.ReadAsync(request.ClientId);
            if (client == null)
            {
                return new ErrorResponse<WriteResponse>(ErrorMessages.CantFindClient);
            }

            var keyPair = client.KeyPair.ToX25519KeyPair();

            string requestJson;
            try
            {
                requestJson = Services.Crypto.Decrypt(request.Body, keyPair).ToUtfString();
            }
            catch
            {
                return new ErrorResponse<WriteResponse>(ErrorMessages.CantDecryptPayload);
            }

            var requestPayload = requestJson.FromJson<WriteRequest.Payload>();
            if (VerifyPayload<WriteResponse>(requestPayload) is { } error)
            {
                return error;
            }

            if (requestPayload.Binary.Length > DataLimit)
            {
                return new ErrorResponse<WriteResponse>(ErrorMessages.DataExceedsLimit);
            }

            var data = await Services.DataRepository.ReadAsync(client.DataId);
            if (data == null)
            {
                return new ErrorResponse<WriteResponse>(ErrorMessages.CantFindData);
            }

            var version = Guid.NewGuid();

            data.Binary = requestPayload.Binary;
            data.Version = version;
            data.LastAccessedAt = Services.Clock.Timestamp;
            await Services.DataRepository.UpdateAsync(data);

            var responsePayload = new WriteResponse.Payload
            {
                Version = version,
                Timestamp = Services.Clock.Timestamp
            };
            var responseBody = Services.Crypto.Encrypt(
                responsePayload.ToJson().ToUtfBytes(),
                keyPair);

            return new WriteResponse
            {
                Body = responseBody
            };
        }
    }
}
