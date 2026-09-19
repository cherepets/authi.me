using Authi.App.Logic.ViewModels;
using System;

namespace Authi.App.Maui.UI;

public partial class OnboardingView
{
    public OnboardingViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            _viewModel = value;
            BindingContext = value;
        }
    }

    private OnboardingViewModel? _viewModel;

    public OnboardingView()
    {
        InitializeComponent();
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            await _viewModel.ContinueOnboardingAsync();
        }
    }

}
