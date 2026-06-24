using Authi.App.Logic.ViewModels;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace Authi.App.Maui.UI;

public partial class MenuBarView
{
    public IMenuBarViewModel? ViewModel
    {
        get => _viewModel;
        set
        {
            OnViewModelChanged(_viewModel, value);
            _viewModel = value;
        }
    }

    private IMenuBarViewModel? _viewModel;

    public MenuBarView()
    {
        InitializeComponent();
    }

    private async void OnLoaded(object sender, EventArgs e)
    {
        await Task.Delay(AnimationLength.Long);
        await ProgressBar.FadeToAsync(1, AnimationLength.DefaultUnsigned, Easing.CubicOut);
    }

    private void OnViewModelChanged(IMenuBarViewModel? _, IMenuBarViewModel? newViewModel)
    {
        BindingContext = newViewModel;
    }

    private void OnSettingsClicked(object sender, EventArgs e)
    {
        _viewModel?.ShowSettings();
    }
}