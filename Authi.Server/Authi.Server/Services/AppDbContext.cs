using Authi.Common.Extensions;
using Authi.Common.Services;
using Authi.Server.Extensions;
using Authi.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Authi.Server.Services
{
    [Service]
    internal interface IAppDbContext
    {
        Task<TEntity?> FindAsync<TEntity>(Guid id) where TEntity : class;
        Task<IReadOnlyCollection<TEntity>> FindAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
        Task InsertAsync<TEntity>(TEntity entity) where TEntity : class;
        Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class;
        Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class;
        Task CleanUpAsync();
    }

    internal class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext()
        {
            Database.EnsureCreated();
        }

        public async Task<TEntity?> FindAsync<TEntity>(Guid id) where TEntity : class
        {
            return await Set<TEntity>().FindAsync(id);
        }

        public async Task<IReadOnlyCollection<TEntity>> FindAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            return await Set<TEntity>().Where(predicate).ToListAsync();
        }

        public async Task InsertAsync<TEntity>(TEntity entity) where TEntity : class
        {
            Set<TEntity>().Add(entity);
            await SaveChangesAsync();
        }

        public async Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class
        {
            Set<TEntity>().Update(entity);
            await SaveChangesAsync();
        }

        public async Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class
        {
            Set<TEntity>().Remove(entity);
            await SaveChangesAsync();
        }

        public async Task CleanUpAsync()
        {
            var clock = ServiceProvider.Current.Get<IClock>();
            var syncTimeStamp = clock.UniversalTime.AddDays(-1).ToUnixTimeMilliseconds();
            var dataTimeStamp = clock.UniversalTime.AddDays(-365).ToUnixTimeMilliseconds();

            var clientSet = Set<Client>();
            var syncSet = Set<Sync>();
            var dataSet = Set<Data>();

            await clientSet
                .Where(c => dataSet.Any(d => d.DataId == c.DataId && d.LastAccessedAt < dataTimeStamp))
                .ExecuteDeleteAsync();
            await syncSet
                .Where(x => x.CreatedAt < syncTimeStamp)
                .ExecuteDeleteAsync();
            await dataSet
                .Where(x => x.LastAccessedAt < dataTimeStamp)
                .ExecuteDeleteAsync();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var healthMonitor = ServiceProvider.Current.Get<IAppHealthMonitor>();
            /* OMITTED IN OSS BUILD */
            optionsBuilder
                .UseMySql(
                    connectionString: "",
                    serverVersion: new MySqlServerVersion(""))
                .OnError(healthMonitor.ReportEvent);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var keyPairConverter = new ValueConverter<KeyPair, string>(
                o => o.ToJson(),
                j => j.FromJson<KeyPair>()!);

            modelBuilder.Entity<Client>()
                .ToTable("client")
                .HasKey(c => c.ClientId);
            modelBuilder.Entity<Client>()
                .Property(c => c.KeyPair)
                .HasConversion(keyPairConverter)
                .HasColumnType("json");

            modelBuilder.Entity<Data>()
                .ToTable("data")
                .HasKey(c => c.DataId);

            modelBuilder.Entity<Sync>()
                .ToTable("sync")
                .HasKey(c => c.SyncId);
            modelBuilder.Entity<Sync>()
                .Property(c => c.OneTimeKeyPair)
                .HasConversion(keyPairConverter)
                .HasColumnType("json");
        }
    }
}
