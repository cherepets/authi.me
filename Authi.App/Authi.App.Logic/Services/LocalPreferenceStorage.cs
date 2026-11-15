using AotServiceProvider;
using Authi.App.Logic.Data;
using Authi.Common.Extensions;
using LiteDB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authi.App.Logic.Services
{
    [Service]
    internal interface ILocalPreferenceStorage
    {
        Task InsertAsync(Preference data);
        Task UpdateAsync(Preference data);
        Task DeleteAsync(Preference data);
        Task<IReadOnlyCollection<Preference>> GetAllAsync();
    }

    public class LocalPreferenceStorage : LocalStorageBase, ILocalPreferenceStorage
    {
        private readonly ILiteCollection<Preference> _collection;

        public LocalPreferenceStorage()
        {
            _collection = Db.GetCollection<Preference>();
        }

        public Task InsertAsync(Preference data)
            => AsyncTransaction(() => _collection.Insert(data));

        public Task UpdateAsync(Preference data)
            => AsyncTransaction(() => _collection.Update(data));

        public Task DeleteAsync(Preference data)
            => AsyncTransaction(() => _collection.Delete(data.Key));

        public Task<IReadOnlyCollection<Preference>> GetAllAsync()
            => AsyncTransaction(() => _collection.FindAll().ToReadOnly());
    }
}
