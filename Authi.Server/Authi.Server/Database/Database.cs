using Authi.Common.Services;
using System;
using System.Threading.Tasks;

namespace Authi.Server.Database
{
    [Service]
    internal interface IDatabase
    {
        IDatabaseScope CreateScope();
        Task<long> GetSizeAsync();
        Task<bool> CanConnectAsync(string connectionString);
    }

    internal class Database : IDatabase
    {
        public IDatabaseScope CreateScope()
        {
            return new DatabaseScope();
        }

        public async Task<long> GetSizeAsync()
        {
            var configuration = ServiceProvider.Current.Get<Services.IConfiguration>();
            await using var connection = new MySqlConnector.MySqlConnection(configuration.ConnectionString);
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT COALESCE(SUM(data_length + index_length), 0)
                FROM information_schema.tables
                WHERE table_schema = DATABASE()
                """;

            return Convert.ToInt64(await command.ExecuteScalarAsync());
        }

        public async Task<bool> CanConnectAsync(string connectionString)
        {
            try
            {
                await using var connection = new MySqlConnector.MySqlConnection(connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
