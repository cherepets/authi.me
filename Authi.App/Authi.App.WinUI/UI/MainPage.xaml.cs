using Authi.App.Logic.Services;
using Authi.App.Logic.ViewModels;
using Authi.App.WinUI.Extensions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Authi.App.WinUI.UI
{
    public sealed partial class MainPage : IAdaptiveView, IDialogManager
    {
        private const double SizeTrigger = 720;

        public MainPageViewModel ViewModel { get; }

        private bool? _isCompact;

        public MainPage()
        {
            DialogManager.Register(this);
            InitializeComponent();
            ContentFrameCompactBackground.Translation = new Vector3(0, 0, 8);
            ViewModel = new();
            ViewModel.ContentChanged += OnContentChanged;
        }

        public void SetCompactSize(bool isCompact)
        {
            _isCompact = isCompact;
            UpdateLeftColumnVisibility(ContentFrameGrid.Visibility == Visibility.Visible);

            if (isCompact)
            {
                Grid.SetColumn(ContentFrameGridContainer, 0);
                MainGridFirstColumn.Width = new GridLength(1, GridUnitType.Star);
                MainGridSecondColumn.Width = new GridLength(0, GridUnitType.Pixel);
                ContentFrameCompactBackground.Visibility = Visibility.Collapsed;
                ContentFrameGridContainer.Margin = new Thickness(0, 32, 0, 0);
            }
            else
            {
                Grid.SetColumn(ContentFrameGridContainer, 1);
                MainGridFirstColumn.Width = new GridLength(320, GridUnitType.Pixel);
                MainGridSecondColumn.Width = new GridLength(1, GridUnitType.Star);
                ContentFrameCompactBackground.Visibility = Visibility.Visible;
                ContentFrameGridContainer.Margin = new Thickness(16, 96, 0, 0);
            }

            MenuBar.SetCompactSize(isCompact);

            if (ContentFrame.Content is IAdaptiveView adaptiveContent)
            {
                adaptiveContent.SetCompactSize(isCompact);
            }
        }

        public async Task ShowDialogAsync(string title, string message, string? primaryButtonText, string? cancelButtonText, Action? onPrimary, Action? onCancel)
        {
            await DialogPresenter.ShowDialogAsync(title, message, primaryButtonText, cancelButtonText, onPrimary, onCancel);
        }

        private async void OnBackButtonPressed()
        {
            if (!App.Current.IsForeground)
            {
                return;
            }

            if (ContentFrameGrid.Visibility == Visibility.Visible)
            {
                ViewModel.HideContent();
            }
        }

        private async void OnContentChanged(ViewModelBase? viewModel)
        {
            switch (viewModel)
            {
                case ICredentialEditorViewModel credentialEditorViewModel:
                    await ShowContent(new CredentialEditorView
                    {
                        ViewModel = credentialEditorViewModel
                    });
                    break;
                case SettingsViewModel settingsViewModel:
                    await ShowContent(new SettingsView
                    {
                        ViewModel = settingsViewModel
                    });
                    break;
                default:
                    await HideContent();
                    break;
            }
        }

        private async Task ShowContent(Control view)
        {
            if (view is IAdaptiveView adaptiveView && _isCompact is bool isCompact)
            {
                adaptiveView.SetCompactSize(isCompact);
            }
            if (ContentFrame.Content is IDisposable disposable)
            {
                disposable.Dispose();
            }
            if (_isCompact != true)
            {
                var animateFrameOnly = ContentFrameGrid.Visibility == Visibility.Visible;
                UIElement element = animateFrameOnly ? ContentFrame : ContentFrameGrid;
                await Animator
                    .Blocking()
                    .OnStart(() =>
                    {
                        UpdateLeftColumnVisibility(true);
                        ContentFrameGrid.Visibility = Visibility.Visible;
                        ContentFrame.Content = view;
                    })
                    .From(
                        (element, Animator.Property.TranslateX, 0),
                        (element, Animator.Property.TranslateY, ContentFrameGridContainer.ActualHeight))
                    .To(
                        (element, Animator.Property.TranslateY, 0, Animator.Length.Short, Animator.Easing.In))
                    .RunAsync();
            }
            else
            {
                await Animator
                    .Blocking()
                    .OnStart(() =>
                    {
                        ContentFrameGrid.Visibility = Visibility.Visible;
                        ContentFrame.Content = view;
                    })
                    .From(
                        (HeaderBackground, Animator.Property.TranslateY, 0),
                        (MenuBar, Animator.Property.TranslateY, 0),
                        (CredentialsCollectionView, Animator.Property.TranslateY, 0),
                        (ContentFrameGrid, Animator.Property.TranslateX, ContentFrameGridContainer.ActualWidth),
                        (ContentFrameGrid, Animator.Property.TranslateY, 0))
                    .To(
                        (HeaderBackground, Animator.Property.TranslateY, -HeaderBackground.ActualHeight, Animator.Length.Default, Animator.Easing.Out),
                        (MenuBar, Animator.Property.TranslateY, -MenuBar.ActualHeight, Animator.Length.Default, Animator.Easing.Out),
                        (CredentialsCollectionView, Animator.Property.TranslateY, CredentialsCollectionView.ActualHeight, Animator.Length.Default, Animator.Easing.Out),
                        (ContentFrameGrid, Animator.Property.TranslateX, 0, Animator.Length.Default, Animator.Easing.In))
                    .RunAsync();
                UpdateLeftColumnVisibility(true);
            }
        }

        private async Task HideContent()
        {
            if (_isCompact != true)
            {
                await Animator
                    .Blocking()
                    .From(
                        (ContentFrameGrid, Animator.Property.TranslateX, 0),
                        (ContentFrameGrid, Animator.Property.TranslateY, 0))
                    .To(
                        (ContentFrameGrid, Animator.Property.TranslateY, ContentFrameGridContainer.ActualHeight, Animator.Length.Default, Animator.Easing.Out))
                    .RunAsync();
            }
            else
            {
                await Animator
                    .Blocking()
                    .OnStart(() =>
                    {
                        UpdateLeftColumnVisibility(false);
                    })
                    .From(
                        (HeaderBackground, Animator.Property.TranslateY, -HeaderBackground.ActualHeight),
                        (MenuBar, Animator.Property.TranslateY, -MenuBar.ActualHeight),
                        (CredentialsCollectionView, Animator.Property.TranslateY, CredentialsCollectionView.ActualHeight),
                        (ContentFrameGrid, Animator.Property.TranslateX, 0),
                        (ContentFrameGrid, Animator.Property.TranslateY, 0))
                    .To(
                        (HeaderBackground, Animator.Property.TranslateY, 0, Animator.Length.Default, Animator.Easing.In),
                        (MenuBar, Animator.Property.TranslateY, 0, Animator.Length.Default, Animator.Easing.In),
                        (CredentialsCollectionView, Animator.Property.TranslateY, 0, Animator.Length.Default, Animator.Easing.In),
                        (ContentFrameGrid, Animator.Property.TranslateX, ContentFrameGridContainer.ActualWidth, Animator.Length.Default, Animator.Easing.Out))
                    .RunAsync();
            }
            if (ContentFrame.Content is IDisposable disposable)
            {
                disposable.Dispose();
            }
            ContentFrameGrid.Visibility = Visibility.Collapsed;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_isCompact != true && ActualWidth < SizeTrigger)
            {
                SetCompactSize(true);
            }
            if (_isCompact != false && ActualWidth >= SizeTrigger)
            {
                SetCompactSize(false);
            }
        }

        private async void UpdateLeftColumnVisibility(bool isRightColumnVisible)
        {
            var isLeftColumnVisibile = _isCompact != true || !isRightColumnVisible;
            HeaderBackground.Visibility = MenuBar.Visibility = CredentialsCollectionView.Visibility
                = isLeftColumnVisibile ? Visibility.Visible : Visibility.Collapsed;
            if (isLeftColumnVisibile)
            {
                await Animator
                    .Blocking()
                    .From(
                        (HeaderBackground, Animator.Property.TranslateY, 0),
                        (MenuBar, Animator.Property.TranslateY, 0),
                        (CredentialsCollectionView, Animator.Property.TranslateY, 0))
                    .RunAsync();
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            ShortcutsEnable();
            await ViewModel.InitializeAsync();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ShortcutsDisable();
            ViewModel.Dispose();
        }
    }
}
