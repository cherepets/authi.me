using Authi.Common.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Authi.App.Logic.ViewModels
{
    public interface IMenuBarViewModel
    {
        event Action<int> TotpRefreshed;

        SyncViewModel SyncViewModel { get; }
        Task ShowAddCredentialsAsync();
        Task ShowSettingsAsync();
    }

    public interface ICopyNotificationViewModel
    {
        event Action? TotpCopied;
    }

    public interface ICredentialsCollectionViewModel : ICopyNotificationViewModel
    {
        ObservableCollection<CredentialViewModel> Credentials { get; }
    }

    public class MainPageViewModel : ViewModelBase, IMenuBarViewModel, ICredentialsCollectionViewModel, ICopyNotificationViewModel, IDisposable
    {
        public event Action<int>? TotpRefreshed;
        public event Action? TotpCopied;
        public event Action<ViewModelBase?>? ContentChanged;

        public ObservableCollection<CredentialViewModel> Credentials { get; }
        public OnboardingViewModel OnboardingViewModel { get; } = new();
        public SyncViewModel SyncViewModel { get; } = new();

        private CancellationTokenSource? _calcDelayCancellation;
        private ViewModelBase? _contentViewModel;
        private bool _isDisposed;

        public MainPageViewModel()
        {
            Credentials = SyncViewModel.GetCredentials();

            Services.Messenger.Copied.Subscribe += OnCopyRequested;
            Services.Messenger.CalcNow.Subscribe += OnCalcNowRequested;
            Services.Messenger.NavigationPop.Subscribe += OnNavigationPopped;
            Services.Messenger.NavigationPush.Subscribe += OnNavigationPushed;
            Services.Messenger.DeeplinkActivated.Subscribe += OnDeeplinkActivated;
            Services.Messenger.DeleteCredential.Subscribe += OnDeleteCredentialRequested;
        }

        public async Task InitializeAsync()
        {
            await SyncViewModel.InitializeAsync();
            await OnboardingViewModel.InitializeAsync(Credentials.Any());
            UpdateLoop();
        }

        public async Task ShowAddCredentialsAsync()
        {
            if (OnboardingViewModel.OnboardingPage != OnboardingPage.None)
            {
                await OnboardingViewModel.ContinueOnboardingAsync();
                return;
            }
            ShowContent(new AddCredentialViewModel(Credentials));
        }

        public async Task ShowSettingsAsync()
        {
            if (OnboardingViewModel.OnboardingPage != OnboardingPage.None)
            {
                await OnboardingViewModel.ContinueOnboardingAsync();
                return;
            }
            ShowContent(new SettingsViewModel());
        }

        public void HideContent()
        {
            ShowContent(null);
        }

        private async void UpdateLoop()
        {
            while (!_isDisposed)
            {
                var validFor = Services.TotpGenerator.GetRemainingMs();

                TotpRefreshed?.Invoke(validFor);

                try
                {
                    _calcDelayCancellation = new CancellationTokenSource();
                    await Task.Delay(validFor, _calcDelayCancellation.Token);
                }
                catch (OperationCanceledException)
                {
                    _calcDelayCancellation = null;
                }

                await OnTick();
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

        private void OnCalcNowRequested(object? sender, EventArgs e)
        {
            _calcDelayCancellation?.Cancel(true);
        }

        private void OnNavigationPushed(object? sender, ViewModelBase viewModel)
        {
            ShowContent(viewModel);
        }

        private void OnNavigationPopped(object? sender, EventArgs e)
        {
            ShowContent(null);
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

        private void ShowContent(ViewModelBase? viewModel)
        {
            (_contentViewModel as IDisposable)?.Dispose();
            _contentViewModel = viewModel;
            ContentChanged?.Invoke(viewModel);
        }

        public void Dispose()
        {
            _isDisposed = true;

            SyncViewModel.Dispose();
            (_contentViewModel as IDisposable)?.Dispose();
            Services.Messenger.Copied.Subscribe -= OnCopyRequested;
            Services.Messenger.CalcNow.Subscribe -= OnCalcNowRequested;
            Services.Messenger.NavigationPop.Subscribe -= OnNavigationPopped;
            Services.Messenger.NavigationPush.Subscribe -= OnNavigationPushed;
            Services.Messenger.DeeplinkActivated.Subscribe -= OnDeeplinkActivated;
            Services.Messenger.DeleteCredential.Subscribe -= OnDeleteCredentialRequested;
        }
    }
}
