using Authi.App.Logic.Data;
using Authi.Common.Extensions;
using System.Collections.ObjectModel;
using L10n = Authi.App.Logic.Localization;

namespace Authi.App.Logic.ViewModels
{
    public class AddCredentialViewModel : CredentialEditorViewModelBase, ICredentialEditorViewModel, IClosableViewModel
    {
        public override string PageTitle => L10n.Credential.AddPageTitle;

        private readonly ObservableCollection<CredentialViewModel> _credentials;

        public AddCredentialViewModel(ObservableCollection<CredentialViewModel> credentials)
            : base(new Credential())
        {
            _credentials = credentials;
        }

        public async override void Save()
        {
            Secret = Secret.Replace(" ", string.Empty);
            this.MapPropertiesTo(Model);
            Model.Timestamp = Services.Clock.Timestamp;

            await Services.LocalCredentialStorage.InsertAsync(Model);
            _credentials.Add(new CredentialViewModel(Model));
            Services.Messenger.NavigationPop.Publish(this);
            Services.Messenger.SyncNow.Publish(this);
        }
    }
}
