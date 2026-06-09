using Authi.Common.Extensions;
using Authi.Common.Services;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Authi.Server.Services
{
    [Service]
    internal interface IConfiguration
    {
        bool Exists { get; }
        string ConnectionString { get; }
        string CredentialHash { get; }
        string TotpSecret { get; }

        Task SaveAsync(string connectionString, string login, string password, string totpSecret);
    }

    internal class Configuration : IConfiguration
    {
        public bool Exists => _data != null;

        public string ConnectionString =>
            _data?.ConnectionString ??
            throw new MissingConfigurationException();

        public string CredentialHash =>
            _data?.CredentialHash ??
            throw new MissingConfigurationException();

        public string TotpSecret =>
            _data?.TotpSecret ??
            throw new MissingConfigurationException();

        private readonly string _filePath;
        private ConfigurationData? _data;

        public Configuration()
        {
            _filePath = Path.Combine(AppContext.BaseDirectory, "config.json");
            if (File.Exists(_filePath))
            {
                _data = File
                    .ReadAllText(_filePath)
                    .FromJson<ConfigurationData>();
            }
        }

        public async Task SaveAsync(
            string connectionString,
            string login,
            string password,
            string totpSecret)
        {
            var credentialBytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{login}:{password}"));
            var credentialHash = Convert.ToBase64String(credentialBytes);

            var data = new ConfigurationData(
                connectionString,
                credentialHash,
                totpSecret);

            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }

            await File.WriteAllTextAsync(_filePath, data.ToJson());
            _data = data;
        }

        private record ConfigurationData(
            string ConnectionString,
            string CredentialHash,
            string TotpSecret);

        private class MissingConfigurationException : Exception
        {
            public MissingConfigurationException() : base("Server configuration does not exist.") { }
        }
    }
}
