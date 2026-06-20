using Authi.Common.Extensions;
using L10n = Authi.App.Logic.Localization;

namespace Authi.App.Logic.ViewModels
{
    public class EditCredentialViewModel : CredentialEditorViewModelBase, ICredentialEditorViewModel, IClosableViewModel
    {
        private readonly CredentialViewModel _originalVM;

        public override string PageTitle => L10n.Credential.EditPageTitle;

        public EditCredentialViewModel(CredentialViewModel viewModel)
            : base(viewModel.Model)
        {
            _originalVM = viewModel;
        }

        public async override void Save()
        {
            Secret = Secret.Replace(" ", string.Empty);
            this.MapPropertiesTo(Model);
            Model.Timestamp = Services.Clock.Timestamp;

            _originalVM.UpdateModel(Model);

            await Services.LocalCredentialStorage.UpdateAsync(Model);
            Services.Messenger.NavigationPop.Publish(this);
            Services.Messenger.SyncNow.Publish(this);
        }
    }
}
