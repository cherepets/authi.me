using Authi.App.Logic.ViewModels;
using Microsoft.UI.Xaml;

namespace Authi.App.WinUI.UI
{
    public sealed partial class OnboardingView
    {
        public OnboardingViewModel? ViewModel { get; set; }

        public OnboardingView()
        {
            InitializeComponent();
        }

        private void OnButtonLoaded(object sender, RoutedEventArgs e)
        {
            (sender as UIElement)?.Focus(FocusState.Programmatic);
        }
    }
}
