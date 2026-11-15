using Authi.Common.Extensions;
using Authi.App.Logic.Data;
using Authi.App.Logic.Services;
using LiteDB;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authi.App.Test.Mocks
{
    internal class MockRemovalStorage(IEnumerable<Removal> _removalItems) : ILocalRemovalStorage
    {
        public MockRemovalStorage() : this([])
        {
        }

        private readonly List<Removal> _removalList = [.. _removalItems ?? []];

        public Task InsertAsync(Removal removal)
        {
            var copy = new Removal();
            removal.MapPropertiesTo(copy);
            _removalList.Add(copy);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Removal removal)
        {
            var found = _removalList.FirstOrDefault(x => x.CloudId == removal.CloudId);
            Assert.IsNotNull(found);
            _removalList.Remove(found);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Removal>> GetAllAsync()
            => Task.FromResult(_removalList.ToReadOnly());
    }
}
