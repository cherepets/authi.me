using Authi.App.Logic.ViewModels;

namespace Authi.App.Maui.UI;

public partial class CredentialsCollectionView
{
    public ICredentialsCollectionViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            OnViewModelChanged(_viewModel, value);
            _viewModel = value;
        }
    }

    private ICredentialsCollectionViewModel? _viewModel;

    public CredentialsCollectionView()
    {
        InitializeComponent();
    }

    private void OnViewModelChanged(ICredentialsCollectionViewModel? oldViewModel, ICredentialsCollectionViewModel? newViewModel)
    {
        BindingContext = newViewModel;
    }
}
