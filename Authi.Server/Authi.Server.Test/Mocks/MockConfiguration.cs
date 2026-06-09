using Authi.Server.Services;
using System.Threading.Tasks;

namespace Authi.Server.Test.Mocks
{
    internal class MockConfiguration(
        string credentialHash,
        string totpSecret) : IConfiguration
    {
        public bool Exists => true;
        public string ConnectionString => string.Empty;
        public string CredentialHash { get; } = credentialHash;
        public string TotpSecret { get; } = totpSecret;

        public Task SaveAsync(
            string connectionString,
            string login,
            string password,
            string totpSecret)
            => Task.CompletedTask;
    }
}
