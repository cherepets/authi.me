using Authi.App.Logic.Data;
using Authi.App.Logic.Services;
using Authi.Common.Client.Exceptions;
using Authi.Common.Extensions;
using Authi.Common.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Authi.App.Test.Mocks.MockCredentialStorage;

namespace Authi.App.Test.Mocks
{
    internal class MockCredentialStorage(IEnumerable<Credential>? credentials, ThrowsOn throwsOn = ThrowsOn.None) : ILocalCredentialStorage, ICloudCredentialStorage
    {
        public MockCredentialStorage() : this(null)
        {
        }

        [Flags]
        internal enum ThrowsOn
        {
            None = 0,
            Insert = 1,
            Update = 2,
            Delete = 4,
            GetAll = 8
        }

        public static Guid CreateCloudId(int id) => GuidConverter.ToGuid(id);
        public static ObjectId CreateLocalId(int id) => new(0, 0, 0, id);

        private readonly List<Credential> _credentialList = [.. credentials ?? []];

        public async Task<bool> IsConnectedAsync()
        {
            var clientId = await ServiceProvider.Current.Get<ISettings>().ClientId.GetAsync();
            return clientId != null;
        }

        public Task InsertAsync(Credential credential)
        {
            ThrowIfNeeded(ThrowsOn.Insert);

            if (credential.LocalId != null)
            {
                credential.CloudId ??= CreateCloudId(credential.LocalId.Increment);
            }
            if (credential.CloudId != null)
            {
                credential.LocalId ??= CreateLocalId(GuidConverter.ToInt(credential.CloudId.Value));
            }

            var copy = new Credential();
            credential.MapPropertiesTo(copy);
            _credentialList.Add(copy);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Credential credential)
        {
            ThrowIfNeeded(ThrowsOn.Delete);

            var found = _credentialList.FirstOrDefault(x
                => x.CloudId == credential.CloudId
                || x.LocalId == credential.LocalId);
            Assert.IsNotNull(found);
            _credentialList.Remove(found);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Credential credential)
        {
            ThrowIfNeeded(ThrowsOn.Update);

            var found = _credentialList.FirstOrDefault(x
                => x.CloudId == credential.CloudId
                || x.LocalId == credential.LocalId);
            Assert.IsNotNull(found);
            credential.MapPropertiesTo(found);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Credential>> GetAllAsync()
        {
            ThrowIfNeeded(ThrowsOn.GetAll);
            return Task.FromResult(_credentialList.ToReadOnly());
        }

        public Task CommitAsync()
        {
            return Task.CompletedTask;
        }

        private void ThrowIfNeeded(ThrowsOn operation)
        {
            if (throwsOn.HasFlag(operation))
            {
                throwsOn = ThrowsOn.None;
                throw new ApiException("Can't " + operation);
            }
        }
    }

    public static class GuidConverter
    {
        public static Guid ToGuid(int value)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(value).CopyTo(bytes, 0);
            return new Guid(bytes);
        }

        public static int ToInt(Guid guid)
        {
            var bytes = guid.ToByteArray();
            var intBytes = new byte[4];
            Array.Copy(bytes, 0, intBytes, 0, 4);
            return BitConverter.ToInt32(intBytes, 0);
        }
    }
}
