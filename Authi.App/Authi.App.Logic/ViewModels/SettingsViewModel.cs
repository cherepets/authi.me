using Authi.App.Logic.Exceptions;
using Authi.Common.Client;
using Authi.Common.Extensions;
using Authi.Common.Services;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using L10n = Authi.App.Logic.Localization;

namespace Authi.App.Logic.ViewModels
{
    public enum CloudSyncState
    {
        Off,
        On,
        SetUp,
        Download
    }

    public class SettingsViewModel : ViewModelBase, IClosableViewModel
    {
        public bool IsLoading
        {
            get => Get<bool>();
            private set => Set(value);
        }

        public bool IsSynced
        {
            get => Get<bool>();
            private set => Set(value);
        }

        public CloudSyncState SyncState
        {
            get => Get<CloudSyncState>();
            set => Set(value);
        }

        public bool IsUISyncEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public SettingsViewModel()
        {
            IsLoading = true;
            InitAsync();
        }

        private async void InitAsync()
        {
            var clientId = await Services.Settings.ClientId.GetAsync();
            IsLoading = false;
            IsUISyncEnabled = IsSynced = clientId.HasValue;
            SyncState = IsSynced ? CloudSyncState.On : CloudSyncState.Off;
            Services.Messenger.BackupExportComplete.Subscribe += OnExportCompleted;
            Services.Messenger.BackupImportComplete.Subscribe += OnImportCompleted;
        }

        public void UISyncToggled(bool value)
        {
            if (!IsSynced && value)
            {
                SyncState = CloudSyncState.SetUp;
            }
            else if (!value)
            {
                if (IsSynced)
                {
                    Services.DialogManager.ShowDialogAsync(
                        title: L10n.Generic.Confirm,
                        message: L10n.Settings.CloudSyncConfirmDisable,
                        primaryButtonText: L10n.Generic.Yes,
                        cancelButtonText: L10n.Generic.No,
                        onPrimary: DisableSync,
                        onCancel: () =>
                        {
                            SyncState = CloudSyncState.On;
                            IsUISyncEnabled = true;
                        });
                }
                SyncState = CloudSyncState.Off;
            }
        }

        public async void Upload()
        {
            IsLoading = true;
            try
            {
                var result = await Services.ApiClient.InitAsync();

                await Services.Settings.ClientId.SetAsync(result.ClientId);
                await Services.Settings.DataKey.SetAsync(result.DataKey);
                await Services.Settings.SyncPrivateKey.SetAsync(result.SyncKeyPair.Private);
                await Services.Settings.SyncPublicKey.SetAsync(result.SyncKeyPair.Public);

                SyncState = CloudSyncState.On;
                IsSynced = true;
                Services.Messenger.SyncNow.Publish(this);
                await Services.DialogManager.ShowDialogAsync(
                    title: L10n.Generic.Success,
                    message: L10n.Settings.CloudSyncSuccess,
                    cancelButtonText: L10n.Generic.Close);
            }
            catch
            {
                IsUISyncEnabled = IsSynced = false;
                SyncState = CloudSyncState.Off;
                await Services.DialogManager.ShowDialogAsync(
                    title: L10n.Generic.Error,
                    message: L10n.Settings.CloudSyncFailure,
                    cancelButtonText: L10n.Generic.Close);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void Download()
        {
            SyncState = CloudSyncState.Download;
        }

        public async void CopySyncCode()
        {
            var code = await GetSyncCodeAsync();
            if (code != null)
            {
                var base64 = code.ToBase64String();
                await Services.Clipboard.SetTextAsync(base64);
            }
        }

        public async Task<byte[]?> GetSyncCodeAsync()
        {
            IsLoading = true;
            try
            {
                var clientId = await Services.Settings.ClientId.GetAsync();
                var version = await Services.Settings.Version.GetAsync();
                var dataEncryptionKey = await Services.Settings.DataKey.GetAsync();
                var syncPrivateKey = await Services.Settings.SyncPrivateKey.GetAsync();
                var syncPublicKey = await Services.Settings.SyncPublicKey.GetAsync();

                if (!clientId.HasValue || dataEncryptionKey == null || syncPrivateKey == null || syncPublicKey == null)
                {
                    throw new MissingSettingsException();
                }

                var syncKeyPair = new X25519KeyPair(
                    new X25519PrivateKey(syncPrivateKey),
                    new X25519PublicKey(syncPublicKey));

                var result = await Services.ApiClient.PublishAsync(clientId.Value, syncKeyPair);

                var code = new SyncCode
                {
                    SyncId = result.SyncId,
                    DataKey = new AesKey(await Services.Settings.DataKey.GetAsync()),
                    OneTimeKey = result.OneTimeKey
                }.Serialize();
                return code;
            }
            catch (Exception exception)
            {
                Services.Logger.Write(exception);
                return null;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async void PasteSyncCode()
        {
            var text = await Services.Clipboard.GetTextAsync();
            if (!string.IsNullOrEmpty(text))
            {
                byte[] bytes;
                try
                {
                    bytes = text.ToBase64Bytes();
                }
                catch
                {
                    return;
                }
                await ConsumeAsync(bytes);
            }
        }

        public Task QrScanned(byte[] bytes)
        {
            return ConsumeAsync(bytes);
        }

        public void Close()
        {
            if (IsSynced != IsUISyncEnabled)
            {
                Services.DialogManager.ShowDialogAsync(
                    title: L10n.Generic.Confirm,
                    message: L10n.Settings.UnsavedChanges,
                    primaryButtonText: L10n.Generic.Yes,
                    cancelButtonText: L10n.Generic.No,
                    onPrimary: () => Services.Messenger.NavigationPop.Publish(this));
            }
            else
            {
                Services.Messenger.NavigationPop.Publish(this);
            }
        }

        public void Export()
        {
            IsLoading = true;
            Services.Messenger.BackupExportInitiate.Publish(this);
        }

        public async Task Import()
        {
            IsLoading = true;
            string? data;

            try
            {
                using var stream = await Services.FileSystem.ReadFromPickerAsync();
                if (stream == null)
                {
                    IsLoading = false;
                    return;
                }
                using var reader = new StreamReader(stream);
                data = await reader.ReadToEndAsync();
            }
            catch (Exception exception)
            {
                Services.Logger.Write(exception);
                IsLoading = false;
                return;
            }

            if (string.IsNullOrEmpty(data))
            {
                IsLoading = false;
                return;
            }

            Services.Messenger.BackupImportInitiate.Publish(this, data);
        }

        public async Task GetApp()
        {
            await Services.LinkOpener.OpenUriAsync(L10n.Settings.GetAppLinkUrl);
        }

        private async void OnExportCompleted(object? sender, string data)
        {
            using var stream = new MemoryStream(Encoding.Default.GetBytes(data));
            var date = DateTime.Now.Date.ToString("yyyy-MM-dd");
            var fileName = $"authi-{date}.bak";
            try
            {
                if (await Services.FileSystem.WriteToPickerAsync(stream, fileName))
                {
                    await Services.DialogManager.ShowDialogAsync(
                        title: L10n.Generic.Success,
                        message: L10n.Settings.BackupExportSuccess,
                        cancelButtonText: L10n.Generic.Close);
                }
            }
            catch (Exception exception)
            {
                Services.Logger.Write(exception);
                await Services.DialogManager.ShowDialogAsync(
                    title: L10n.Generic.Error,
                    message: L10n.Settings.BackupExportFailure,
                    cancelButtonText: L10n.Generic.Close);
            }
            IsLoading = false;
        }

        private void OnImportCompleted(object? sender, EventArgs e)
        {
            IsLoading = false;
        }

        private async Task ConsumeAsync(byte[] bytes)
        {
            if (IsLoading) return;

            IsLoading = true;
            try
            {
                var syncCode = SyncCode.Deserialize(bytes);

                var result = await Services.ApiClient.ConsumeAsync(syncCode);

                await Services.Settings.ClientId.SetAsync(result.ClientId);
                await Services.Settings.DataKey.SetAsync(result.DataKey);
                await Services.Settings.SyncPrivateKey.SetAsync(result.SyncKeyPair.Private);
                await Services.Settings.SyncPublicKey.SetAsync(result.SyncKeyPair.Public);

                SyncState = CloudSyncState.On;
                IsSynced = true;
                Services.Messenger.SyncNow.Publish(this);
                await Services.DialogManager.ShowDialogAsync(
                    title: L10n.Generic.Success,
                    message: L10n.Settings.CloudSyncSuccess,
                    cancelButtonText: L10n.Generic.Close);
            }
            catch (Exception exception)
            {
                Services.Logger.Write(exception);
                // Probably wrong qr, ignoring
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void DisableSync()
        {
            IsLoading = true;
            try
            {
                // TODO: Delete from server
                //Services.CloudCredentialStorage.DisableAsync();
                await Services.Settings.ClientId.SetAsync(null);
                IsSynced = false;
                Services.Messenger.SyncNow.Publish(this);
            }
            catch (Exception exception)
            {
                Services.Logger.Write(exception);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
