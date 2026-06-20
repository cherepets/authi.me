using Authi.App.Logic.Services;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;

namespace Authi.App.WinUI.Services
{
    internal class SecureStorage : ISecureStorage
    {
        private ApplicationDataContainer DataContainer
        {
            get
            {
                if (_dataContainer != null)
                {
                    return _dataContainer;
                }

                var name = Logic.Localization.Generic.AppName;
                var localSettings = ApplicationData.Current.LocalSettings;
                if (!localSettings.Containers.ContainsKey(name))
                    localSettings.CreateContainer(name, ApplicationDataCreateDisposition.Always);
                _dataContainer = localSettings.Containers[name];
                return _dataContainer;
            }
        }

        private ApplicationDataContainer? _dataContainer;

        public async Task<string?> GetAsync(string key)
        {
            var bytes = GetBytesAsync(key);

            if (bytes == null)
                return null;

            if (bytes.Length != 0)
            {
                var provider = new DataProtectionProvider();
                var buffer = await provider.UnprotectAsync(bytes.AsBuffer());
                bytes = buffer.ToArray();
            }

            return Encoding.UTF8.GetString(bytes);
        }

        public async Task SetAsync(string key, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);

            if (bytes.Length == 0)
            {
                Remove(key);
                return;
            }

            // LOCAL=user and LOCAL=machine do not require enterprise auth capability
            var provider = new DataProtectionProvider("LOCAL=user");
            var buffer = await provider.ProtectAsync(bytes.AsBuffer());
            bytes = buffer.ToArray();

            SetBytesAsync(key, bytes);
        }

        public void Remove(string key)
        {
            DataContainer.Values.Remove(key);
        }

        private byte[]? GetBytesAsync(string key)
        {
            return DataContainer.Values[key] as byte[];
        }

        private void SetBytesAsync(string key, byte[] data)
        {
            DataContainer.Values[key] = data;
        }
    }
}
