using Authi.App.Logic.ViewModels;
using Authi.Common.Services;
using System;

namespace Authi.App.Logic.Services
{
    [Service]
    public interface IMessenger
    {
        Channel Copied { get; }
        Channel SyncNow { get; }
        Channel NavigationPop { get; }
        Channel<ViewModelBase> NavigationPush { get; }
        Channel<CredentialViewModel> DeleteCredential { get; }
        Channel BackupExportInitiate { get; }
        Channel<string> BackupExportComplete { get; }
        Channel<string> BackupImportInitiate { get; }
        Channel BackupImportComplete { get; }
        Channel<string> DeeplinkActivated { get; }
    }

    public class Messenger : ServiceBase, IMessenger
    {
        public Channel Copied { get; } = new();
        public Channel SyncNow { get; } = new();
        public Channel NavigationPop { get; } = new();
        public Channel<ViewModelBase> NavigationPush { get; } = new();
        public Channel<CredentialViewModel> DeleteCredential { get; } = new();
        public Channel BackupExportInitiate { get; } = new();
        public Channel<string> BackupExportComplete { get; } = new();
        public Channel<string> BackupImportInitiate { get; } = new();
        public Channel BackupImportComplete { get; } = new();
        public Channel<string> DeeplinkActivated { get; } = new();
    }

    public class Channel
    {
        public event EventHandler? Subscribe;

        public void Publish(object sender)
        {
            Subscribe?.Invoke(sender, new EventArgs());
        }
    }

    public class Channel<T>
    {
        public event EventHandler<T>? Subscribe;

        public void Publish(object sender, T message)
        {
            Subscribe?.Invoke(sender, message);
        }
    }
}
