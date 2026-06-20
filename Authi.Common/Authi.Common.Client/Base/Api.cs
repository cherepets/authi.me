using Authi.Common.Client.Exceptions;
using Authi.Common.Dto;
using Authi.Common.Extensions;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Authi.Common.Client
{
    internal class Api : IDisposable
    {
        private readonly HttpClient _httpClient;

        public Api(string serverUrl) 
        {
            _httpClient = new()
            {
                BaseAddress = new Uri($"https://{serverUrl}/api/v1/")
            };
        }

        public Task<ConsumeResponse> ConsumeAsync(ConsumeRequest request)
        {
            return Execute<ConsumeRequest, ConsumeResponse>("consume", request);
        }

        // TODO: Why is it not used?
        public Task<DeleteResponse> DeleteAsync(DeleteRequest request)
        {
            return Execute<DeleteRequest, DeleteResponse>("delete", request);
        }

        public Task<InitResponse> InitAsync(InitRequest request)
        {
            return Execute<InitRequest, InitResponse>("init", request);
        }

        public Task<PublishResponse> PublishAsync(PublishRequest request)
        {
            return Execute<PublishRequest, PublishResponse>("publish", request);
        }

        public Task<ReadResponse> ReadAsync(ReadRequest request)
        {
            return Execute<ReadRequest, ReadResponse>("read", request);
        }

        public Task<WriteResponse> WriteAsync(WriteRequest request)
        {
            return Execute<WriteRequest, WriteResponse>("write", request);
        }

        private async Task<TResponse> Execute<TRequest, TResponse>(string uri, TRequest request)
            where TResponse : OptionalResponse<TResponse>
        {
            try
            {
                var httpContent = new StringContent(request.ToJson(), Encoding.UTF8, "application/json");
                var httpResponse = await _httpClient.PostAsync(uri, httpContent);
                if (httpResponse.IsSuccessStatusCode)
                {
                    var responseJson = await httpResponse.Content.ReadAsStringAsync();
                    var optional = responseJson?.FromJson<OptionalResponse<TResponse>>()
                        ?? throw new ApiException("Http response has no content.");
                    if (optional.Error != null)
                    {
                        throw new ApiException($"Server side error: {optional.Error}");
                    }
                    var response = responseJson.FromJson<TResponse>();
                    if (response?.Result == null)
                    {
                        throw new ApiException("Can't parse server result.");
                    }
                    return response;
                }
                else
                {
                    throw new ApiException($"Http response status code [{httpResponse.StatusCode}]");
                }
            }
            catch (Exception exception) when (exception is not ApiException)
            {
                throw new ApiException($"Api call failed: {exception.Message}");
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
