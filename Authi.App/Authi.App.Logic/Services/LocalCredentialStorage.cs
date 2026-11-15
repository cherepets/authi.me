using AotServiceProvider;
using Authi.Common.Extensions;
using Authi.App.Logic.Data;
using LiteDB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authi.App.Logic.Services
{
    [Service]
    internal interface ILocalCredentialStorage : ICredentialStorage
    {
    }

    public class LocalCredentialStorage : LocalStorageBase, ILocalCredentialStorage
    {
        private readonly ILiteCollection<Credential> _collection;

        public LocalCredentialStorage()
        {
            _collection = Db.GetCollection<Credential>();
        }

        public Task InsertAsync(Credential data)
            => AsyncTransaction(() => _collection.Insert(data));

        public Task UpdateAsync(Credential data)
            => AsyncTransaction(() => _collection.Update(data));

        public Task DeleteAsync(Credential data)
            => AsyncTransaction(() => _collection.Delete(data.LocalId));

        public Task<IReadOnlyCollection<Credential>> GetAllAsync()
            => AsyncTransaction(() => _collection.FindAll().ToReadOnly());
    }
}
