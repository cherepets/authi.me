using System;
using System.Threading.Tasks;

namespace Authi.Server.Services
{
    internal interface IRepository<T> where T : class
    {
        Task CreateAsync(T client);
        Task<T?> ReadAsync(Guid id);
        Task UpdateAsync(T client);
        Task DeleteAsync(T client);
    }
}
