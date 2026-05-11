using Authi.Common.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Authi.App.Logic.ViewModels
{
    public interface IMenuBarViewModel
    {
        event Action<int> TotpRefreshed;

        SyncViewModel SyncViewModel { get; }
        void ShowAddCredentials();
        void ShowSettings();
    }

    public interface ICopyNotificationViewModel
    {
        event Action? TotpCopied;
    }

    public interface ICredentialsCollectionViewModel : ICopyNotificationViewModel
    {
        ObservableCollection<CredentialViewModel> Credentials { get; }
        void ShowAddCredentials();
    }

    public class MainPageViewModel : ViewModelBase, IMenuBarViewModel, ICredentialsCollectionViewModel, ICopyNotificationViewModel, IDisposable
    {
        public event Action<int>? TotpRefreshed;
        public event Action? TotpCopied;
        public event Action<ViewModelBase?>? ContentChanged;
        public ObservableCollection<CredentialViewModel> Credentials { get; }
        public SyncViewModel SyncViewModel { get; } = new();

        public MainPageViewModel()
        {
            Credentials = SyncViewModel.GetCredentials();

            Services.Messenger.Copied.Subscribe += OnCopyRequested;
            Services.Messenger.NavigationPush.Subscribe += OnNavigationPushed;
            Services.Messenger.NavigationPop.Subscribe += OnNavigationPopped;
            Services.Messenger.DeleteCredential.Subscribe += OnDeleteCredentialRequested;
            Services.Messenger.DeeplinkActivated.Subscribe += OnDeeplinkActivated;
        }

        public async Task InitializeAsync()
        {
            await SyncViewModel.InitializeAsync();
            UpdateLoop();
        }

        public void ShowAddCredentials()
        {
            ShowContent(new AddCredentialViewModel(Credentials));
        }

        public void ShowSettings()
        {
            ShowContent(new SettingsViewModel());
        }

        private async void UpdateLoop()
        {
            var validFor = Services.TotpGenerator.GetRemainingMs();
            while (true)
            {
                TotpRefreshed?.Invoke(validFor);
                await Task.Delay(validFor);
                var task = OnTick();
                validFor = Config.UpdateMs;
                await task;
            }
        }

        private async Task OnTick()
        {
            var updates = await Task.Run(CalculateCodes);
            var chunkSize = updates.Count > Config.ChunkCount
                ? updates.Count / Config.ChunkCount
                : Config.ChunkCount;
            while (true)
            {
                for (var i = 0; i < chunkSize; i++)
                {
                    if (!updates.TryDequeue(out var update))
                    {
                        return;
                    }
                    update();
                    await Task.Delay(Config.ChunkMs);
                }
            }
        }

        private Queue<Action> CalculateCodes()
        {
            var credentials = new Queue<CredentialViewModel>(Credentials);
            var queue = new Queue<Action>(credentials.Count);
            while (credentials.TryDequeue(out var credential))
            {
                var totp = credential.CalculateTotp();
                queue.Enqueue(() => credential.Totp = totp);
            }
            return queue;
        }

        private void OnCopyRequested(object? sender, EventArgs e)
        {
            TotpCopied?.Invoke();
        }

        private void OnNavigationPushed(object? sender, ViewModelBase viewModel)
        {
            ShowContent(viewModel);
        }

        private void OnNavigationPopped(object? sender, EventArgs e)
        {
            HideContent();
        }

        private async void OnDeleteCredentialRequested(object? sender, CredentialViewModel credentialViewModel)
        {
            await SyncViewModel.DeleteAsync(credentialViewModel);
        }

        private void OnDeeplinkActivated(object? sender, string e)
        {
            if (OtpauthUri.TryParse(e, out var otpauth))
            {
                ShowContent(new AddCredentialViewModel(Credentials)
                {
                    Title = otpauth.Issuer,
                    Secret = otpauth.Secret,
                    Subtitle = otpauth.Account ?? string.Empty,
                });
            }
        }

        private void ShowContent(ViewModelBase viewModel)
        {
            ContentChanged?.Invoke(viewModel);
        }

        private void HideContent()
        {
            ContentChanged?.Invoke(null);
        }

        public void Dispose()
        {
            SyncViewModel.Dispose();
            Services.Messenger.Copied.Subscribe -= OnCopyRequested;
            Services.Messenger.NavigationPush.Subscribe -= OnNavigationPushed;
            Services.Messenger.NavigationPop.Subscribe -= OnNavigationPopped;
            Services.Messenger.DeleteCredential.Subscribe -= OnDeleteCredentialRequested;
        }
    }
}
