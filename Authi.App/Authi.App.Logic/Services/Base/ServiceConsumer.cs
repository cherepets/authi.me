using Authi.Common.Services;

namespace Authi.App.Logic.Services
{
    internal interface IServiceConsumer
    {
        IServiceProvider ServiceProvider { get; }

        public IApiClient ApiClient => ServiceProvider.Get<IApiClient>();
        public IBinarySerializer BinarySerializer => ServiceProvider.Get<IBinarySerializer>();
        public IBiometrics Biometrics => ServiceProvider.Get<IBiometrics>();
        public IClipboard Clipboard => ServiceProvider.Get<IClipboard>();
        public IClock Clock => ServiceProvider.Get<IClock>();
        public ICloudCredentialStorage CloudCredentialStorage => ServiceProvider.Get<ICloudCredentialStorage>();
        public ICrypto Crypto => ServiceProvider.Get<ICrypto>();
        public IDialogManager DialogManager => ServiceProvider.Get<IDialogManager>();
        public IFileSystem FileSystem => ServiceProvider.Get<IFileSystem>();
        public ILinkOpener LinkOpener => ServiceProvider.Get<ILinkOpener>();
        public ILocalCredentialStorage LocalCredentialStorage => ServiceProvider.Get<ILocalCredentialStorage>();
        public ILocalPreferenceStorage LocalPreferenceStorage => ServiceProvider.Get<ILocalPreferenceStorage>();
        public ILogger Logger => ServiceProvider.Get<ILogger>();
        public IMessenger Messenger => ServiceProvider.Get<IMessenger>();
        public ILocalRemovalStorage RemovalStorage => ServiceProvider.Get<ILocalRemovalStorage>();
        public ISecureStorage SecureStorage => ServiceProvider.Get<ISecureStorage>();
        public ISettings Settings => ServiceProvider.Get<ISettings>();
        public ITotpGenerator TotpGenerator => ServiceProvider.Get<ITotpGenerator>();
    }
}
