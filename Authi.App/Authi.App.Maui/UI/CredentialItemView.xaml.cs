using Authi.App.Logic.ViewModels;
using System;

namespace Authi.App.Maui.UI;

public partial class CredentialItemView
{
    private CredentialViewModel? _viewModel;

    public CredentialItemView()
    {
        InitializeComponent();
    }

    private void OnBindingContextChanged(object sender, EventArgs e)
    {
        _viewModel = (CredentialViewModel)BindingContext;
    }

    private void OnClicked(object sender, EventArgs e)
    {
        _viewModel?.CopyToClipboard();
    }

    private void OnEditClicked(object sender, EventArgs e)
    {
        _viewModel?.Edit();
    }

    private void OnDeleteClicked(object sender, EventArgs e)
    {
        _viewModel?.Delete();
    }

    private void OptionsClicked(object sender, EventArgs e)
    {
        if (_viewModel is CredentialViewModel viewModel)
        {
            viewModel.IsEditing = !viewModel.IsEditing;
        }
    }
}