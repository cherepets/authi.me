using Authi.App.Logic.Data;
using Authi.App.Logic.Services;
using Authi.App.Logic.ViewModels;
using Authi.App.Test.Mocks;
using Authi.Common.Client.Exceptions;
using Authi.Common.Extensions;
using Authi.Common.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static Authi.App.Test.Mocks.MockCredentialStorage;

namespace Authi.App.Test
{
    [TestClass]
    public class SyncViewModelTests : AppTestsBase
    {
        private static ObjectId LocalId1 => CreateLocalId(1);
        private static ObjectId LocalId2 => CreateLocalId(2);
        private static Guid CloudId1 => CreateCloudId(1);
        private static Guid CloudId2 => CreateCloudId(2);

        private const string Title1 = "Title 1";
        private const string Title2 = "Title 2";
        private const string Secret1 = "AEAQCAIBAEAQCAIB";
        private const string Secret2 = "AIBAEAQCAIBAEAQC";
        private const string Totp1 = "123456";
        private const string Totp2 = "987654";
        private const string DisplayTotp1 = "123 456";
        private const string DisplayTotp2 = "987 654";
        private const long Timestamp1 = 1;
        private const long Timestamp2 = 2;

        [TestMethod]
        public async Task SyncNotSyncedTest()
        {
            ConfigureServices(
                localCredentials: [
                    new Credential
                    {
                        LocalId = LocalId1,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    }],
                cloudCredentials: [],
                cloudIdsForRemoval: [],
                isOnline: false);

            var syncVM = new SyncViewModel();

            Assert.AreEqual(SyncStatus.NotSynced, syncVM.Status);

            await syncVM.InitializeAsync();

            var credentials = syncVM.GetCredentials();
            var removalItems = await Services.RemovalStorage.GetAllAsync();
            var local = await Services.LocalCredentialStorage.GetAllAsync();
            var cloud = await Services.CloudCredentialStorage.GetAllAsync();

            Assert.AreEqual(SyncStatus.NotSynced, syncVM.Status);

            Assert.IsNull(syncVM.SyncError);
            Assert.HasCount(1, local);
            Assert.IsEmpty(cloud);
            Assert.HasCount(1, credentials);
            Assert.IsEmpty(removalItems);

            Assert.AreEqual(Totp1, credentials[0].Totp);
            Assert.AreEqual(Title1, credentials[0].Title);
            Assert.AreEqual(DisplayTotp1, credentials[0].DisplayTotp);
        }

        [TestMethod]
        public async Task SyncInsertErrorTest()
        {
            ConfigureServices(
                localCredentials: [
                    new Credential
                    {
                        LocalId = LocalId1,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    }],
                cloudCredentials: [],
                cloudIdsForRemoval: [],
                isOnline: true,
                ThrowsOn.Insert);

            var syncVM = new SyncViewModel();

            Assert.AreEqual(SyncStatus.Offline, syncVM.Status);

            await syncVM.InitializeAsync();

            var credentials = syncVM.GetCredentials();
            var removalItems = await Services.RemovalStorage.GetAllAsync();
            var local = await Services.LocalCredentialStorage.GetAllAsync();
            var cloud = await Services.CloudCredentialStorage.GetAllAsync();

            Assert.AreEqual(SyncStatus.Error, syncVM.Status);

            Assert.IsInstanceOfType<ApiException>(syncVM.SyncError);
            Assert.AreEqual("Can't Insert", syncVM.SyncError.Message);
            Assert.HasCount(1, local);
            Assert.IsEmpty(cloud);
            Assert.HasCount(1, credentials);
            Assert.IsEmpty(removalItems);

            Assert.AreEqual(Totp1, credentials[0].Totp);
            Assert.AreEqual(Title1, credentials[0].Title);
            Assert.AreEqual(DisplayTotp1, credentials[0].DisplayTotp);
        }

        [TestMethod]
        public async Task SyncUpdateErrorTest()
        {
            ConfigureServices(
                localCredentials: [
                    new Credential
                    {
                        LocalId = LocalId2,
                        CloudId = CloudId2,
                        Title = Title2,
                        Secret = Secret2,
                        Timestamp = Timestamp2
                    }],
                cloudCredentials: [
                    new Credential
                    {
                        CloudId = CloudId2,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    }],
                cloudIdsForRemoval: [],
                isOnline: true,
                ThrowsOn.Update);

            var syncVM = new SyncViewModel();

            Assert.AreEqual(SyncStatus.Offline, syncVM.Status);

            await syncVM.InitializeAsync();

            var credentials = syncVM.GetCredentials();
            var removalItems = await Services.RemovalStorage.GetAllAsync();
            var local = await Services.LocalCredentialStorage.GetAllAsync();
            var cloud = await Services.CloudCredentialStorage.GetAllAsync();

            Assert.AreEqual(SyncStatus.Error, syncVM.Status);

            Assert.IsInstanceOfType<ApiException>(syncVM.SyncError);
            Assert.AreEqual("Can't Update", syncVM.SyncError.Message);
            Assert.HasCount(1, local);
            Assert.HasCount(1, cloud);
            Assert.HasCount(1, credentials);
            Assert.IsEmpty(removalItems);

            Assert.AreEqual(Totp2, credentials[0].Totp);
            Assert.AreEqual(Title2, credentials[0].Title);
            Assert.AreEqual(DisplayTotp2, credentials[0].DisplayTotp);
        }

        [TestMethod]
        public async Task SyncDeleteErrorTest()
        {
            ConfigureServices(
                localCredentials: [],
                cloudCredentials: [
                    new Credential
                    {
                        CloudId = CloudId1,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    }],
                cloudIdsForRemoval: [CloudId1],
                isOnline: true,
                ThrowsOn.Delete);

            var syncVM = new SyncViewModel();

            Assert.AreEqual(SyncStatus.Offline, syncVM.Status);

            await syncVM.InitializeAsync();

            var credentials = syncVM.GetCredentials();
            var removalItems = await Services.RemovalStorage.GetAllAsync();
            var local = await Services.LocalCredentialStorage.GetAllAsync();
            var cloud = await Services.CloudCredentialStorage.GetAllAsync();

            Assert.AreEqual(SyncStatus.Error, syncVM.Status);

            Assert.IsInstanceOfType<ApiException>(syncVM.SyncError);
            Assert.AreEqual("Can't Delete", syncVM.SyncError.Message);
            Assert.IsEmpty(local);
            Assert.HasCount(1, cloud);
            Assert.IsEmpty(credentials);
            Assert.HasCount(1, removalItems);
        }

        [TestMethod]
        public async Task SyncGetAllErrorTest()
        {
            ConfigureServices(
                localCredentials: [
                    new Credential
                    {
                        LocalId = LocalId1,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    }],
                cloudCredentials: [],
                cloudIdsForRemoval: [],
                isOnline: true,
                ThrowsOn.GetAll);

            var syncVM = new SyncViewModel();

            Assert.AreEqual(SyncStatus.Offline, syncVM.Status);

            await syncVM.InitializeAsync();

            var credentials = syncVM.GetCredentials();
            var removalItems = await Services.RemovalStorage.GetAllAsync();
            var local = await Services.LocalCredentialStorage.GetAllAsync();
            var cloud = await Services.CloudCredentialStorage.GetAllAsync();

            Assert.AreEqual(SyncStatus.Error, syncVM.Status);

            Assert.IsInstanceOfType<ApiException>(syncVM.SyncError);
            Assert.AreEqual("Can't GetAll", syncVM.SyncError.Message);
            Assert.HasCount(1, local);
            Assert.IsEmpty(cloud);
            Assert.HasCount(1, credentials);
            Assert.IsEmpty(removalItems);

            Assert.AreEqual(Totp1, credentials[0].Totp);
            Assert.AreEqual(Title1, credentials[0].Title);
            Assert.AreEqual(DisplayTotp1, credentials[0].DisplayTotp);
        }

        [TestMethod]
        public async Task SyncIncomingTest()
        {
            ConfigureServices(
                localCredentials: [
                    new Credential
                    {
                        LocalId = LocalId1,
                        CloudId = CloudId1,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    }],
                cloudCredentials: [
                    new Credential
                    {
                        CloudId = CloudId1,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    },
                    new Credential
                    {
                        CloudId = CloudId2,
                        Title = Title2,
                        Secret = Secret2,
                        Timestamp = Timestamp2
                    }],
                cloudIdsForRemoval: [],
                isOnline: true);

            var syncVM = new SyncViewModel();

            Assert.AreEqual(SyncStatus.Offline, syncVM.Status);

            await syncVM.InitializeAsync();

            var credentials = syncVM.GetCredentials();
            var removalItems = await Services.RemovalStorage.GetAllAsync();
            var local = await Services.LocalCredentialStorage.GetAllAsync();
            var cloud = await Services.CloudCredentialStorage.GetAllAsync();

            Assert.AreEqual(SyncStatus.Synced, syncVM.Status);

            Assert.IsNull(syncVM.SyncError);
            Assert.HasCount(2, local);
            Assert.HasCount(2, cloud);
            Assert.HasCount(2, credentials);
            Assert.IsEmpty(removalItems);

            Assert.AreEqual(CloudId1, credentials[0].Model.CloudId);
            Assert.AreEqual(Totp1, credentials[0].Totp);
            Assert.AreEqual(Title1, credentials[0].Title);
            Assert.AreEqual(DisplayTotp1, credentials[0].DisplayTotp);

            Assert.AreEqual(CloudId2, credentials[1].Model.CloudId);
            Assert.AreEqual(Totp2, credentials[1].Totp);
            Assert.AreEqual(Title2, credentials[1].Title);
            Assert.AreEqual(DisplayTotp2, credentials[1].DisplayTotp);

            Assert.AreEqual(Timestamp1, local.ToArray()[0].Timestamp);
            Assert.AreEqual(Timestamp2, local.ToArray()[1].Timestamp);
            Assert.AreEqual(Timestamp1, cloud.ToArray()[0].Timestamp);
            Assert.AreEqual(Timestamp2, cloud.ToArray()[1].Timestamp);
        }

        [TestMethod]
        public async Task SyncOutgoingTest()
        {
            ConfigureServices(
                localCredentials: [
                    new Credential
                    {
                        LocalId = LocalId1,
                        CloudId = CloudId1,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    },
                    new Credential
                    {
                        LocalId = LocalId2,
                        Title = Title2,
                        Secret = Secret2,
                        Timestamp = Timestamp2
                    }],
                cloudCredentials: [
                    new Credential
                    {
                        CloudId = CloudId1,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    }],
                cloudIdsForRemoval: [],
                isOnline: true);

            var syncVM = new SyncViewModel();

            Assert.AreEqual(SyncStatus.Offline, syncVM.Status);

            await syncVM.InitializeAsync();

            var credentials = syncVM.GetCredentials();
            var removalItems = await Services.RemovalStorage.GetAllAsync();
            var local = await Services.LocalCredentialStorage.GetAllAsync();
            var cloud = await Services.CloudCredentialStorage.GetAllAsync();

            Assert.AreEqual(SyncStatus.Synced, syncVM.Status);

            Assert.IsNull(syncVM.SyncError);
            Assert.HasCount(2, local);
            Assert.HasCount(2, cloud);
            Assert.HasCount(2, credentials);
            Assert.IsEmpty(removalItems);

            Assert.AreEqual(CloudId1, credentials[0].Model.CloudId);
            Assert.AreEqual(Totp1, credentials[0].Totp);
            Assert.AreEqual(Title1, credentials[0].Title);
            Assert.AreEqual(DisplayTotp1, credentials[0].DisplayTotp);

            Assert.AreEqual(CloudId2, credentials[1].Model.CloudId);
            Assert.AreEqual(Totp2, credentials[1].Totp);
            Assert.AreEqual(Title2, credentials[1].Title);
            Assert.AreEqual(DisplayTotp2, credentials[1].DisplayTotp);

            Assert.AreEqual(Timestamp1, local.ToArray()[0].Timestamp);
            Assert.AreEqual(Timestamp2, local.ToArray()[1].Timestamp);
            Assert.AreEqual(Timestamp1, cloud.ToArray()[0].Timestamp);
            Assert.AreEqual(Timestamp2, cloud.ToArray()[1].Timestamp);
        }

        [TestMethod]
        public async Task SyncConflictTest()
        {
            ConfigureServices(
                localCredentials: [
                    // Older
                    new Credential
                    {
                        LocalId = LocalId1,
                        CloudId = CloudId1,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    },
                    // Newer
                    new Credential
                    {
                        LocalId = LocalId2,
                        CloudId = CloudId2,
                        Title = Title2,
                        Secret = Secret2,
                        Timestamp = Timestamp2
                    }],
                cloudCredentials: [
                    // Newer
                    new Credential
                    {
                        CloudId = CloudId1,
                        Title = Title2,
                        Secret = Secret2,
                        Timestamp = Timestamp2
                    },
                    // Older
                    new Credential
                    {
                        CloudId = CloudId2,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    }],
                cloudIdsForRemoval: [],
                isOnline: true);

            var syncVM = new SyncViewModel();

            Assert.AreEqual(SyncStatus.Offline, syncVM.Status);

            await syncVM.InitializeAsync();

            var credentials = syncVM.GetCredentials();
            var removalItems = await Services.RemovalStorage.GetAllAsync();
            var local = await Services.LocalCredentialStorage.GetAllAsync();
            var cloud = await Services.CloudCredentialStorage.GetAllAsync();

            Assert.AreEqual(SyncStatus.Synced, syncVM.Status);

            Assert.IsNull(syncVM.SyncError);
            Assert.HasCount(2, local);
            Assert.HasCount(2, cloud);
            Assert.HasCount(2, credentials);
            Assert.IsEmpty(removalItems);

            Assert.AreEqual(CloudId1, credentials[0].Model.CloudId);
            Assert.AreEqual(Totp2, credentials[0].Totp);
            Assert.AreEqual(Title2, credentials[0].Title);
            Assert.AreEqual(DisplayTotp2, credentials[0].DisplayTotp);

            Assert.AreEqual(CloudId2, credentials[1].Model.CloudId);
            Assert.AreEqual(Totp2, credentials[1].Totp);
            Assert.AreEqual(Title2, credentials[1].Title);
            Assert.AreEqual(DisplayTotp2, credentials[1].DisplayTotp);

            Assert.AreEqual(Timestamp2, local.ToArray()[0].Timestamp);
            Assert.AreEqual(Timestamp2, local.ToArray()[1].Timestamp);
            Assert.AreEqual(Timestamp2, cloud.ToArray()[0].Timestamp);
            Assert.AreEqual(Timestamp2, cloud.ToArray()[1].Timestamp);
        }

        [TestMethod]
        public async Task SyncFindsDuplicatesTest()
        {
            ConfigureServices(
                // Missing CloudId
                localCredentials: [
                    new Credential
                    {
                        LocalId = LocalId1,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    },
                    new Credential
                    {
                        LocalId = LocalId2,
                        Title = Title2,
                        Secret = Secret2,
                        Timestamp = Timestamp2
                    }],
                cloudCredentials: [
                    new Credential
                    {
                        CloudId = CloudId1,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    },
                    new Credential
                    {
                        CloudId = CloudId2,
                        Title = Title2,
                        Secret = Secret2,
                        Timestamp = Timestamp2
                    }],
                cloudIdsForRemoval: [],
                isOnline: true);

            var syncVM = new SyncViewModel();

            Assert.AreEqual(SyncStatus.Offline, syncVM.Status);

            await syncVM.InitializeAsync();

            var credentials = syncVM.GetCredentials();
            var removalItems = await Services.RemovalStorage.GetAllAsync();
            var local = await Services.LocalCredentialStorage.GetAllAsync();
            var cloud = await Services.CloudCredentialStorage.GetAllAsync();

            Assert.AreEqual(SyncStatus.Synced, syncVM.Status);

            Assert.IsNull(syncVM.SyncError);
            Assert.HasCount(2, local);
            Assert.HasCount(2, cloud);
            Assert.HasCount(2, credentials);
            Assert.IsEmpty(removalItems);

            Assert.AreEqual(CloudId1, credentials[0].Model.CloudId);
            Assert.AreEqual(Totp1, credentials[0].Totp);
            Assert.AreEqual(Title1, credentials[0].Title);
            Assert.AreEqual(DisplayTotp1, credentials[0].DisplayTotp);

            Assert.AreEqual(CloudId2, credentials[1].Model.CloudId);
            Assert.AreEqual(Totp2, credentials[1].Totp);
            Assert.AreEqual(Title2, credentials[1].Title);
            Assert.AreEqual(DisplayTotp2, credentials[1].DisplayTotp);

            Assert.AreEqual(Timestamp1, local.ToArray()[0].Timestamp);
            Assert.AreEqual(Timestamp2, local.ToArray()[1].Timestamp);
            Assert.AreEqual(Timestamp1, cloud.ToArray()[0].Timestamp);
            Assert.AreEqual(Timestamp2, cloud.ToArray()[1].Timestamp);
        }

        [TestMethod]
        public async Task SyncRemovalTest()
        {
            ConfigureServices(
                localCredentials: [
                    new Credential
                    {
                        LocalId = LocalId1,
                        CloudId = CloudId1,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    }],
                cloudCredentials: [
                    new Credential
                    {
                        CloudId = CloudId2,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    }],
                cloudIdsForRemoval: [CloudId2],
                isOnline: true);

            var syncVM = new SyncViewModel();

            Assert.AreEqual(SyncStatus.Offline, syncVM.Status);

            await syncVM.InitializeAsync();

            var credentials = syncVM.GetCredentials();
            var removalItems = await Services.RemovalStorage.GetAllAsync();
            var local = await Services.LocalCredentialStorage.GetAllAsync();
            var cloud = await Services.CloudCredentialStorage.GetAllAsync();

            Assert.AreEqual(SyncStatus.Synced, syncVM.Status);

            Assert.IsNull(syncVM.SyncError);
            Assert.IsEmpty(local);
            Assert.IsEmpty(cloud);
            Assert.IsEmpty(credentials);
            Assert.IsEmpty(removalItems);
        }

        [TestMethod]
        public async Task DeleteCredentialTest()
        {
            ConfigureServices(
                localCredentials: [
                    new Credential
                    {
                        LocalId = LocalId1,
                        CloudId = CloudId1,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    }],
                cloudCredentials: [
                    new Credential
                    {
                        LocalId = LocalId1,
                        CloudId = CloudId1,
                        Title = Title1,
                        Secret = Secret1,
                        Timestamp = Timestamp1
                    }],
                cloudIdsForRemoval: [],
                isOnline: true);

            var syncVM = new SyncViewModel();
            await syncVM.InitializeAsync();

            await syncVM.DeleteAsync(syncVM.GetCredentials().First());

            var credentials = syncVM.GetCredentials();
            var removalItems = await Services.RemovalStorage.GetAllAsync();
            var local = await Services.LocalCredentialStorage.GetAllAsync();

            Assert.IsNull(syncVM.SyncError);
            Assert.IsEmpty(local);
            Assert.IsEmpty(credentials);
            Assert.HasCount(1, removalItems);
        }

        private void ConfigureServices(Credential[] localCredentials, Credential[] cloudCredentials, Guid[] cloudIdsForRemoval, bool isOnline, ThrowsOn cloudThrowsOn = ThrowsOn.None)
        {
            static Removal ToRemoval(Guid cloudId) => new() { CloudId = cloudId };

            ServicesMock
                .Override<ISettings>(new MockSettings().Customize(async settings =>
                {
                    var emptyKey = new byte[Crypto.AesKeySize];
                    await settings.ClientId.SetAsync(isOnline ? Guid.NewGuid() : null);
                    await settings.SyncPrivateKey.SetAsync(isOnline ? emptyKey : null);
                    await settings.SyncPublicKey.SetAsync(isOnline ? emptyKey : null);
                    await settings.DataKey.SetAsync(isOnline ? emptyKey : null);
                }))
                .Override<ITotpGenerator>(new MockTotpGenerator(new Dictionary<string, string>
                {
                    { Secret1, Totp1 },
                    { Secret2, Totp2 }
                }))
                .Override<ILocalCredentialStorage>(new MockCredentialStorage(localCredentials))
                .Override<ICloudCredentialStorage>(new MockCredentialStorage(cloudCredentials, cloudThrowsOn))
                .Override<ILocalRemovalStorage>(new MockRemovalStorage(cloudIdsForRemoval.Select(ToRemoval)));
        }
    }
}
