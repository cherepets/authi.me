using LiteDB;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Authi.App.Logic.Services
{
    public abstract class LocalStorageBase : ServiceBase
    {
        private static readonly object _transactionLock = new();

        protected static LiteDatabase Db => _db.Value;
        private static readonly Lazy<LiteDatabase> _db = new(() => new LiteDatabase(
            Path.Combine(
                AotServiceProvider.Services.Current.Get<IFileSystem>().AppDataDirectory,
                nameof(Authi))));

        #region Transactions
        protected static Task AsyncTransaction(Action action)
            => Task.Run(() => Transaction(action));

        protected static Task<T> AsyncTransaction<T>(Func<T> func)
            => Task.Run(() => Transaction(func));

        protected static void Transaction(Action action)
        {
            lock (_transactionLock) action();
        }

        protected static T Transaction<T>(Func<T> func)
        {
            lock (_transactionLock) return func();
        }
        #endregion
    }
}
