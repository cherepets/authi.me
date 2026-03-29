using Authi.Common.Dto;
using Authi.Common.Extensions;
using Authi.Common.Services;
using Authi.Common.Test.Mocks;
using Authi.Server.ApiVersions;
using Authi.Server.Models;
using System;
using System.Threading.Tasks;

namespace Authi.Server.Test
{
    [TestClass]
    public class ReadTests : ServerTestsBase
    {
        [TestMethod]
        public async Task ReadHappyTest()
        {
            var api = new ApiV1();

            var clock = new MockClock();
            ServicesMock.Override<IClock>(clock);
            clock.UniversalTime = DateTimeOffset.FromUnixTimeMilliseconds(255);

            var dataKey = Services.Crypto.GenerateAesKey();
            var clientKeyPair = Services.Crypto.GenerateX25519KeyPair();
            var serverKeyPair = Services.Crypto.GenerateX25519KeyPair();
            (clientKeyPair, serverKeyPair) = ExchangePublicKeys(
                clientKeyPair,
                serverKeyPair);

            var dbMessage = "message";

            var clientId = Guid.NewGuid();
            var dataId = Guid.NewGuid();
            var version = Guid.NewGuid();
            var dbData = new Data
            {
                DataId = dataId,
                Version = version,
                Binary = Services.Crypto.Encrypt(dbMessage.ToUtfBytes(), dataKey),
                LastAccessedAt = 0
            };

            var dbClient = new Client
            {
                ClientId = clientId,
                DataId = dataId,
                KeyPair = new KeyPair
                {
                    Private = serverKeyPair.Private.ToString(),
                    Public = serverKeyPair.Public.ToString()
                }
            };

            await DataRepository.CreateAsync(dbData);
            await ClientRepository.CreateAsync(dbClient);

            var requestPayload = new ReadRequest.Payload
            {
                Timestamp = Services.Clock.Timestamp,
                Version = Guid.NewGuid()
            };
            var requestBody = Services.Crypto.Encrypt(
                requestPayload.ToJson().ToUtfBytes(),
                clientKeyPair);
            var request = new ReadRequest
            {
                ClientId = clientId,
                Body = requestBody
            };

            // Call API
            var response = await api.OnRead(request);

            Assert.IsNull(response.Error);
            Assert.IsNotNull(response.Result);

            var responseBody = Services.Crypto.Decrypt(
                response.Result.Body,
                clientKeyPair);
            var responsePayload = responseBody.ToUtfString().FromJson<ReadResponse.Payload>();

            Assert.IsNotNull(responsePayload);
            Assert.AreEqual(255, responsePayload.Timestamp);

            Assert.AreEqual(dbData.Version, responsePayload.Version);
            Assert.IsTrue(responsePayload.HasChanges);

            var responseMessage = Services.Crypto.Decrypt(responsePayload.Binary, dataKey).ToUtfString();

            Assert.AreEqual(dbMessage, responseMessage);
        }

        [TestMethod]
        public async Task ReadHasNoChangesTest()
        {
            var api = new ApiV1();

            var clock = new MockClock();
            ServicesMock.Override<IClock>(clock);
            clock.UniversalTime = DateTimeOffset.FromUnixTimeMilliseconds(255);

            var dataKey = Services.Crypto.GenerateAesKey();
            var clientKeyPair = Services.Crypto.GenerateX25519KeyPair();
            var serverKeyPair = Services.Crypto.GenerateX25519KeyPair();
            (clientKeyPair, serverKeyPair) = ExchangePublicKeys(
                clientKeyPair,
                serverKeyPair);

            var dbMessage = "message";

            var clientId = Guid.NewGuid();
            var dataId = Guid.NewGuid();
            var version = Guid.NewGuid();
            var dbData = new Data
            {
                DataId = dataId,
                Version = version,
                Binary = Services.Crypto.Encrypt(dbMessage.ToUtfBytes(), dataKey),
                LastAccessedAt = 0
            };

            var dbClient = new Client
            {
                ClientId = clientId,
                DataId = dataId,
                KeyPair = new KeyPair
                {
                    Private = serverKeyPair.Private.ToString(),
                    Public = serverKeyPair.Public.ToString()
                }
            };

            await DataRepository.CreateAsync(dbData);
            await ClientRepository.CreateAsync(dbClient);

            var requestPayload = new ReadRequest.Payload
            {
                Timestamp = Services.Clock.Timestamp,
                Version = version
            };
            var requestBody = Services.Crypto.Encrypt(
                requestPayload.ToJson().ToUtfBytes(),
                clientKeyPair);
            var request = new ReadRequest
            {
                ClientId = clientId,
                Body = requestBody
            };

            // Call API
            var response = await api.OnRead(request);

            Assert.IsNull(response.Error);
            Assert.IsNotNull(response.Result);

            var responseBody = Services.Crypto.Decrypt(
                response.Result.Body,
                clientKeyPair);
            var responsePayload = responseBody.ToUtfString().FromJson<ReadResponse.Payload>();

            Assert.IsNotNull(responsePayload);
            Assert.AreEqual(255, responsePayload.Timestamp);

            Assert.AreEqual(requestPayload.Version, responsePayload.Version);
            Assert.IsFalse(responsePayload.HasChanges);

            var responseMessage = Services.Crypto.Decrypt(responsePayload.Binary, dataKey).ToUtfString();

            Assert.AreEqual(string.Empty, responseMessage);
        }

        [TestMethod]
        public async Task ReadCantFindClientTest()
        {
            var api = new ApiV1();

            var clock = new MockClock();
            ServicesMock.Override<IClock>(clock);
            clock.UniversalTime = DateTimeOffset.FromUnixTimeMilliseconds(255);

            var dataKey = Services.Crypto.GenerateAesKey();
            var clientKeyPair = Services.Crypto.GenerateX25519KeyPair();
            var serverKeyPair = Services.Crypto.GenerateX25519KeyPair();
            (clientKeyPair, serverKeyPair) = ExchangePublicKeys(
                clientKeyPair,
                serverKeyPair);

            var dbMessage = "message";

            var clientId = Guid.NewGuid();
            var dataId = Guid.NewGuid();
            var version = Guid.NewGuid();
            var dbData = new Data
            {
                DataId = dataId,
                Version = version,
                Binary = Services.Crypto.Encrypt(dbMessage.ToUtfBytes(), dataKey),
                LastAccessedAt = 0
            };

            // Don't create client
            await DataRepository.CreateAsync(dbData);

            var requestPayload = new ReadRequest.Payload
            {
                Timestamp = Services.Clock.Timestamp,
                Version = Guid.NewGuid()
            };
            var requestBody = Services.Crypto.Encrypt(
                requestPayload.ToJson().ToUtfBytes(),
                clientKeyPair);
            var request = new ReadRequest
            {
                ClientId = clientId,
                Body = requestBody
            };

            // Call API
            var response = await api.OnRead(request);

            Assert.IsNull(response.Result);
            Assert.IsNotNull(response.Error);

            Assert.AreEqual(ErrorMessages.CantFindClient, response.Error);
        }

        [TestMethod]
        public async Task ReadCantDecryptPayloadTest()
        {
            var api = new ApiV1();

            var clock = new MockClock();
            ServicesMock.Override<IClock>(clock);
            clock.UniversalTime = DateTimeOffset.FromUnixTimeMilliseconds(255);

            var dataKey = Services.Crypto.GenerateAesKey();
            var clientKeyPair = Services.Crypto.GenerateX25519KeyPair();
            var serverKeyPair = Services.Crypto.GenerateX25519KeyPair();
            (clientKeyPair, serverKeyPair) = ExchangePublicKeys(
                clientKeyPair,
                serverKeyPair);

            var dbMessage = "message";

            var clientId = Guid.NewGuid();
            var dataId = Guid.NewGuid();
            var version = Guid.NewGuid();
            var dbData = new Data
            {
                DataId = dataId,
                Version = version,
                Binary = Services.Crypto.Encrypt(dbMessage.ToUtfBytes(), dataKey),
                LastAccessedAt = 0
            };

            var dbClient = new Client
            {
                ClientId = clientId,
                DataId = dataId,
                KeyPair = new KeyPair
                {
                    Private = serverKeyPair.Private.ToString(),
                    Public = serverKeyPair.Public.ToString()
                }
            };

            await DataRepository.CreateAsync(dbData);
            await ClientRepository.CreateAsync(dbClient);

            var requestPayload = new ReadRequest.Payload
            {
                Timestamp = Services.Clock.Timestamp,
                Version = Guid.NewGuid()
            };

            // Encrypt with the wrong key
            var requestBody = Services.Crypto.Encrypt(
                requestPayload.ToJson().ToUtfBytes(),
                dataKey);
            var request = new ReadRequest
            {
                ClientId = clientId,
                Body = requestBody
            };

            // Call API
            var response = await api.OnRead(request);

            Assert.IsNull(response.Result);
            Assert.IsNotNull(response.Error);

            Assert.AreEqual(ErrorMessages.CantDecryptPayload, response.Error);
        }

        [TestMethod]
        public async Task ReadCantVerifyClockTest()
        {
            var api = new ApiV1();

            var clock = new MockClock();
            ServicesMock.Override<IClock>(clock);
            clock.UniversalTime = DateTimeOffset.FromUnixTimeSeconds(0);

            var dataKey = Services.Crypto.GenerateAesKey();
            var clientKeyPair = Services.Crypto.GenerateX25519KeyPair();
            var serverKeyPair = Services.Crypto.GenerateX25519KeyPair();
            (clientKeyPair, serverKeyPair) = ExchangePublicKeys(
                clientKeyPair,
                serverKeyPair);

            var dbMessage = "message";

            var clientId = Guid.NewGuid();
            var dataId = Guid.NewGuid();
            var version = Guid.NewGuid();
            var dbData = new Data
            {
                DataId = dataId,
                Version = version,
                Binary = Services.Crypto.Encrypt(dbMessage.ToUtfBytes(), dataKey),
                LastAccessedAt = 0
            };

            var dbClient = new Client
            {
                ClientId = clientId,
                DataId = dataId,
                KeyPair = new KeyPair
                {
                    Private = serverKeyPair.Private.ToString(),
                    Public = serverKeyPair.Public.ToString()
                }
            };

            await DataRepository.CreateAsync(dbData);
            await ClientRepository.CreateAsync(dbClient);

            var requestPayload = new ReadRequest.Payload
            {
                Timestamp = Services.Clock.Timestamp,
                Version = Guid.NewGuid()
            };
            var requestBody = Services.Crypto.Encrypt(
                requestPayload.ToJson().ToUtfBytes(),
                clientKeyPair);
            var request = new ReadRequest
            {
                ClientId = clientId,
                Body = requestBody
            };

            // Set a later time
            clock.UniversalTime = DateTimeOffset.FromUnixTimeSeconds(31);

            // Call API
            var response = await api.OnRead(request);

            Assert.IsNull(response.Result);
            Assert.IsNotNull(response.Error);

            Assert.AreEqual(ErrorMessages.CantVerifyClock, response.Error);
        }
    }
}
