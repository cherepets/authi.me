using Authi.App.Logic.ViewModels;
using Microsoft.Maui;
using System;

namespace Authi.App.Maui.UI;

public partial class CredentialsCollectionView
{
    public ICredentialsCollectionViewModel ViewModel
    {
        get => _viewModel;
        set
        {
            OnViewModelChanged(_viewModel, value);
            _viewModel = value;
        }
    }

    private ICredentialsCollectionViewModel _viewModel;

    public CredentialsCollectionView()
    {
        InitializeComponent();
        var bottom = AddCredentialsButton.HeightRequest + AddCredentialsButton.Margin.VerticalThickness;
        ScrollView.Padding = new Thickness(0, 0, 0, bottom);
    }

    private void OnAddCredentialsClicked(object sender, EventArgs e)
    {
        _viewModel.ShowAddCredentials();
    }

    private void OnViewModelChanged(ICredentialsCollectionViewModel oldViewModel, ICredentialsCollectionViewModel newViewModel)
    {
        BindingContext = newViewModel;
    }
}