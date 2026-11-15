using AotServiceProvider;
using Authi.Common.Extensions;
using Authi.App.Logic.Data;
using LiteDB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authi.App.Logic.Services
{
    [Service]
    internal interface ILocalRemovalStorage
    {
        Task InsertAsync(Removal dto);
        Task DeleteAsync(Removal dto);
        Task<IReadOnlyCollection<Removal>> GetAllAsync();
    }

    public class LocalRemovalStorage : LocalStorageBase, ILocalRemovalStorage
    {
        private readonly ILiteCollection<Removal> _collection;

        public LocalRemovalStorage()
        {
            _collection = Db.GetCollection<Removal>();
        }

        public Task InsertAsync(Removal dto)
            => AsyncTransaction(() => _collection.Insert(dto));

        public Task DeleteAsync(Removal dto)
            => AsyncTransaction(() => _collection.Delete(dto.CloudId));

        public Task<IReadOnlyCollection<Removal>> GetAllAsync()
            => AsyncTransaction(() => _collection.FindAll().ToReadOnly());
    }
}
