using Authi.App.Logic.Data;
using Authi.Common.Client;
using Authi.Common.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using L10n = Authi.App.Logic.Localization;

namespace Authi.App.Logic.ViewModels
{
    public enum SyncStatus
    {
        NotSynced,
        Offline,
        Syncing,
        Synced,
        Error
    }

    public class SyncViewModel : ViewModelBase, IDisposable
    {
        private readonly ObservableCollection<CredentialViewModel> _credentials = [];

        private bool _isDisposed;

        public SyncViewModel()
        {
            InitAsync();
        }

        public SyncStatus Status
        {
            get => Get<SyncStatus>();
            private set => Set(value);
        }

        internal Exception? SyncError { get; private set; }

        private CancellationTokenSource? _syncDelayCancellation;

        internal ObservableCollection<CredentialViewModel> GetCredentials() => _credentials;

        internal async Task InitializeAsync()
        {
            await LoadFromLocalDb();
            StartSync();
        }

        internal async Task DeleteAsync(CredentialViewModel credentialViewModel)
        {
            _credentials.Remove(credentialViewModel);
            if (credentialViewModel.Model.CloudId is Guid cloudId)
            {
                await Services.RemovalStorage.InsertAsync(new Removal { CloudId = cloudId });
            }
            await Services.LocalCredentialStorage.DeleteAsync(credentialViewModel.Model);
        }

        private async void InitAsync()
        {
            var isConnected = await Services.CloudCredentialStorage.IsConnectedAsync();
            Status = isConnected ? SyncStatus.Offline : SyncStatus.NotSynced;

            Services.Messenger.SyncNow.Subscribe += OnSyncNowRequested;
            Services.Messenger.BackupExportInitiate.Subscribe += OnExportInitiated;
            Services.Messenger.BackupImportInitiate.Subscribe += OnImportInitiated;
        }

        private async void StartSync()
        {
            while (!_isDisposed)
            {
                await PerformSyncAsync();

                try
                {
                    _syncDelayCancellation = new CancellationTokenSource();
                    await Task.Delay(Config.SyncPeriodMs, _syncDelayCancellation.Token);
                }
                catch (OperationCanceledException)
                {
                    _syncDelayCancellation = null;
                }
            }
        }

        private async Task PerformSyncAsync()
        {
            SyncError = null;

            var localCopy = _credentials.Select(x => x.Model).ToList();

            var isConnected = await Services.CloudCredentialStorage.IsConnectedAsync();
            if (!isConnected)
            {
                foreach (var localCredential in localCopy)
                {
                    if (localCredential.CloudId != null)
                    {
                        localCredential.CloudId = null;
                        await Services.LocalCredentialStorage.UpdateAsync(localCredential);
                    }
                }
                return;
            }

            try
            {
                Status = SyncStatus.Syncing;
                var cloudCopy = await Services.CloudCredentialStorage.GetAllAsync();
                var removalItems = await Services.RemovalStorage.GetAllAsync();

                foreach (var removalItem in removalItems)
                {
                    await Services.CloudCredentialStorage.DeleteAsync(new Credential { CloudId = removalItem.CloudId });
                    await Services.RemovalStorage.DeleteAsync(removalItem);
                    var credential = cloudCopy.FirstOrDefault(x => x.CloudId == removalItem.CloudId);
                    if (credential != null)
                    {
                        cloudCopy = cloudCopy.Except([credential]).ToReadOnly();
                    }
                }

                foreach (var cloudCredential in cloudCopy)
                {
                    if (localCopy.FirstOrDefault(x => x.CloudId == cloudCredential.CloudId) is Credential localCredential)
                    {
                        await OnCredentialConflictAsync(localCredential, cloudCredential);
                    }
                    else
                    {
                        await OnCredentialIncomingAsync(cloudCredential);
                    }
                }

                foreach (var localCredential in localCopy)
                {
                    if (cloudCopy.FirstOrDefault(x => x.CloudId == localCredential.CloudId) == null)
                    {
                        await OnCredentialOutgoingAsync(localCredential);
                    }
                }

                await Services.CloudCredentialStorage.CommitAsync();

                Status = SyncStatus.Synced;
            }
            catch (Exception exception)
            {
                Status = SyncStatus.Error;
                SyncError = exception;
                return;
            }
        }

        private async Task OnCredentialIncomingAsync(Credential cloudCredential)
        {
            await Services.LocalCredentialStorage.InsertAsync(cloudCredential);
            _credentials.Add(new CredentialViewModel(cloudCredential));
        }

        private async Task OnCredentialConflictAsync(Credential localCredential, Credential cloudCredential)
        {
            if (localCredential.Equals(cloudCredential))
            {
                return;
            }

            if (cloudCredential.Timestamp < localCredential.Timestamp)
            {
                localCredential.MapPropertiesTo(cloudCredential);
                await Services.CloudCredentialStorage.UpdateAsync(cloudCredential);
            }
            else
            {
                cloudCredential.LocalId = localCredential.LocalId;
                cloudCredential.MapPropertiesTo(localCredential);
                await Services.LocalCredentialStorage.UpdateAsync(localCredential);
                var vm = _credentials.First(x => x.Model.LocalId == localCredential.LocalId);
                vm.UpdateModel(localCredential);
            }
        }

        private async Task OnCredentialOutgoingAsync(Credential localCredential)
        {
            if (localCredential.CloudId != null)
            {
                await Services.LocalCredentialStorage.DeleteAsync(localCredential);
                var vm = _credentials.First(x => x.Model.LocalId == localCredential.LocalId);
                _credentials.Remove(vm);
            }
            else
            {
                await Services.CloudCredentialStorage.InsertAsync(localCredential);
                await Services.LocalCredentialStorage.UpdateAsync(localCredential);
            }
        }

        private async Task LoadFromLocalDb()
        {
            var credentials = await Services.LocalCredentialStorage.GetAllAsync();
            foreach (var dto in credentials)
            {
                _credentials.Add(new CredentialViewModel(dto));
            }
        }

        private async void OnSyncNowRequested(object? sender, EventArgs e)
        {
            var isConnected = await Services.CloudCredentialStorage.IsConnectedAsync();
            Status = isConnected ? SyncStatus.Offline : SyncStatus.NotSynced;
            _syncDelayCancellation?.Cancel(true);
        }

        private void OnExportInitiated(object? sender, EventArgs e)
        {
            var backup = string.Join("\r\n", _credentials.Select(x => new OtpauthUri(x.Title, x.Secret, x.Subtitle)));
            Services.Messenger.BackupExportComplete.Publish(this, backup);
        }

        private async void OnImportInitiated(object? sender, string e)
        {
            var splitted = e.Split('\n').Select(x => x.Trim()).ToArray();
            var count = 0;
            foreach (var item in splitted)
            {
                try
                {
                    if (OtpauthUri.TryParse(item, out var uri))
                    {
                        var dto = new Credential
                        {
                            Title = uri.Issuer,
                            Subtitle = uri.Account,
                            Secret = uri.Secret
                        };
                        await Services.LocalCredentialStorage.InsertAsync(dto);
                        _credentials.Add(new CredentialViewModel(dto));
                        count++;
                    }
                }
                catch (Exception exception)
                {
                    Services.Logger.Write(exception);
                }
            }
            Services.Messenger.SyncNow.Publish(this);
            await Services.DialogManager.ShowDialogAsync(
                title: L10n.Generic.Success,
                message: L10n.Settings.BackupImportSuccess,
                cancelButtonText: L10n.Generic.Close);
            Services.Messenger.BackupImportComplete.Publish(this);
        }

        public void Dispose()
        {
            _isDisposed = true;

            Services.Messenger.SyncNow.Subscribe -= OnSyncNowRequested;
            Services.Messenger.BackupExportInitiate.Subscribe -= OnExportInitiated;
            Services.Messenger.BackupImportInitiate.Subscribe -= OnImportInitiated;
        }
    }
}
