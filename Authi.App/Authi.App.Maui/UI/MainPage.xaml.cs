using Authi.App.Logic.Services;
using Authi.App.Logic.ViewModels;
using Authi.App.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using System.Threading.Tasks;

namespace Authi.App.Maui.UI
{
    public partial class MainPage : IAdaptiveView, IDialogManager
    {
        private const double SizeTrigger = 640;

        public MainPageViewModel ViewModel { get; }

        private View _currentView;

        private Task _navigating;

        private bool? _isCompact;
        private bool _isLoaded;

        public MainPage()
        {
            DialogManager.Register(this);
            InitializeComponent();
            ViewModel = new();
            ViewModel.ContentChanged += OnContentChanged;

            MenuBar.ViewModel = ViewModel;
            CredentialsCollection.ViewModel = ViewModel;

            UpdateLeftColumnVisibility();
            UpdateRightColumnVisibility();

            BindingContext = this;
        }

        public void SetCompactSize(bool isCompact)
        {
            _isCompact = isCompact;
            UpdateLeftColumnVisibility();
            UpdateRightColumnVisibility();

            if (isCompact)
            {
                Grid.SetColumn(ContentFrame, 0);
                MainGridFirstColumn.Width = new GridLength(1, GridUnitType.Star);
                MainGridSecondColumn.Width = new GridLength(0, GridUnitType.Absolute);
                ContentFrame.SetDynamicResource(BackgroundProperty, "SurfaceBrush");
            }
            else
            {
                Grid.SetColumn(ContentFrame, 1);
                MainGridFirstColumn.Width = new GridLength(320, GridUnitType.Absolute);
                MainGridSecondColumn.Width = new GridLength(1, GridUnitType.Star);
                ContentFrame.SetDynamicResource(BackgroundProperty, "Surface2Brush");
            }

            if (_currentView is IAdaptiveView currentView)
            {
                currentView.SetCompactSize(isCompact);
            }
        }

        public async Task ShowDialogAsync(string title, string message, string primaryButtonText = null, string cancelButtonText = null, Action onPrimary = null, Action onCancel = null)
        {
            await DialogPresenter.ShowDialogAsync(title, message, primaryButtonText, cancelButtonText, onPrimary, onCancel);
        }

        protected override bool OnBackButtonPressed()
        {
            if (DialogPresenter.IsPresenting)
            {
                DialogPresenter.HideDialog();
                return true;
            }
            if (_currentView?.BindingContext is IClosableViewModel viewModel)
            {
                viewModel.Close();
                return true;
            }
            return base.OnBackButtonPressed();
        }

        private async void OnContentChanged(ViewModelBase viewModel)
        {
            switch (viewModel)
            {
                case ICredentialEditorViewModel credentialEditorViewModel:
                    await ShowContent(new CredentialEditorView
                    {
                        BindingContext = credentialEditorViewModel
                    });
                    break;
                case SettingsViewModel settingsViewModel:
                    await ShowContent(new SettingsView
                    {
                        BindingContext = settingsViewModel
                    });
                    break;
                default:
                    await HideContent();
                    break;
            }
        }

        private async Task ShowContent(View view)
        {
            if (_navigating != null)
            {
                await _navigating;
            }
            var tcs = new TaskCompletionSource();
            _navigating = tcs.Task;

            if (view is IAdaptiveView compactSizeSupporter)
            {
                compactSizeSupporter.SetCompactSize(_isCompact ?? true);
            }

            if (_currentView is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _currentView = view;
            ContentFrameGrid.Children.Clear();
            ContentFrameGrid.Children.Add(view);
            UpdateLeftColumnVisibility();
            UpdateRightColumnVisibility();
            await view.FinishLayoutingAsync();
            if (_isCompact != true)
            {
                ContentFrameGrid.TranslationX = 0;
                ContentFrameGrid.TranslationY = ContentFrame.Height;
            }
            else
            {
                ContentFrameGrid.TranslationX = ContentFrame.Width;
                ContentFrameGrid.TranslationY = 0;
            }
            await ContentFrameGrid.TranslateToAsync(0, 0, AnimationLength.DefaultUnsigned, Easing.CubicOut);
            tcs.SetResult();
        }

        private async Task HideContent()
        {
            if (_navigating != null)
            {
                await _navigating;
            }
            var tcs = new TaskCompletionSource();
            _navigating = tcs.Task;

            if (_isCompact != true)
            {
                await ContentFrameGrid.TranslateToAsync(0, ContentFrame.Height, AnimationLength.DefaultUnsigned, Easing.CubicIn);
            }
            else
            {
                MenuBar.Opacity = 0;
                CredentialsCollection.Scale = 0.25d;
                CredentialsCollection.TranslationY = 240;
                CredentialsCollection.Opacity = 0;
                _ = MenuBar.FadeToAsync(1, AnimationLength.DefaultUnsigned, Easing.CubicIn);
                _ = CredentialsCollection.ScaleToAsync(1, AnimationLength.DefaultUnsigned, Easing.CubicIn);
                _ = CredentialsCollection.TranslateToAsync(0, 0, AnimationLength.DefaultUnsigned, Easing.CubicIn);
                _ = CredentialsCollection.FadeToAsync(1, AnimationLength.DefaultUnsigned, Easing.CubicIn);
                await ContentFrameGrid.TranslateToAsync(ContentFrame.Width, 0, AnimationLength.DefaultUnsigned, Easing.CubicIn);
            }
            var oldContent = _currentView;
            _currentView = null;
            ContentFrameGrid.Children.Clear();
            UpdateLeftColumnVisibility();
            UpdateRightColumnVisibility();
            if (oldContent is IDisposable disposable)
            {
                disposable.Dispose();
            }
            tcs.SetResult();
        }

        private void OnDialogIsPresentingChanged(object sender, bool e)
        {
            Root.IsEnabled = !e;
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            if (_isCompact != true && Width < SizeTrigger)
            {
                SetCompactSize(true);
            }
            if (_isCompact != false && Width >= SizeTrigger)
            {
                SetCompactSize(false);
            }
        }

        private void UpdateLeftColumnVisibility()
        {
            var isLeftColumnVisibile = _isCompact != true || _currentView == null;
            CredentialsCollection.IsVisible = MenuBar.IsVisible = isLeftColumnVisibile;
        }

        private void UpdateRightColumnVisibility()
        {
            var isRightColumnVisibile = _isCompact == false || _currentView != null;
            ContentFrame.IsVisible = isRightColumnVisibile;
        }

        private async void OnLoaded(object sender, EventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                await ViewModel.InitializeAsync();
                await this.FadeToAsync(1, AnimationLength.ShortUnsigned, Easing.CubicIn);
            }
            else
            {
                ViewModel.Dispose();
                AuthiApp.Current.OpenMainPage();
            }
        }
    }
}
