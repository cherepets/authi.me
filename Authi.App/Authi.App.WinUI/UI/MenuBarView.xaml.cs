using Authi.App.Logic.ViewModels;
using Microsoft.UI.Xaml;
using System;

namespace Authi.App.WinUI.UI;

public sealed partial class MenuBarView : IAdaptiveView
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

    public void SetCompactSize(bool isCompact)
    {
        if (isCompact)
        {
            TitleBarFirstColumn.Width = new GridLength(1, GridUnitType.Star);
            TitleBarSecondColumn.Width = new GridLength(0, GridUnitType.Pixel);
        }
        else
        {
            TitleBarFirstColumn.Width = new GridLength(320, GridUnitType.Pixel);
            TitleBarSecondColumn.Width = new GridLength(1, GridUnitType.Star);
        }
    }

    private void OnViewModelChanged(IMenuBarViewModel? oldViewModel, IMenuBarViewModel? newViewModel)
    {
        if (oldViewModel != null)
        {
            oldViewModel.TotpRefreshed -= OnTotpRefreshed;
        }
        if (newViewModel != null)
        {
            newViewModel.TotpRefreshed += OnTotpRefreshed;
        }
    }

    private void OnTotpRefreshed(int milliseconds)
    {
        ProgressBar.UpdateTime(TimeSpan.FromMilliseconds(milliseconds));
    }

    private void OnSettingsClicked(object sender, RoutedEventArgs e)
    {
        ViewModel?.ShowSettingsAsync();
    }

    private void OnAddNewClicked(object sender, RoutedEventArgs e)
    {
        ViewModel?.ShowAddCredentialsAsync();
    }
}
