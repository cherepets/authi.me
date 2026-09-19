using System.Threading.Tasks;

namespace Authi.App.Logic.ViewModels
{
    public enum OnboardingPage
    {
        None,
        Page1,
        Page2
    }

    public class OnboardingViewModel : ViewModelBase
    {
        public OnboardingPage OnboardingPage
        {
            get => Get<OnboardingPage>();
            private set => Set(value);
        }

        public async Task InitializeAsync(bool isExistingUser)
        {
            if (!await DidCompleteOnboardingAsync())
            {
                if (isExistingUser)
                {
                    await Services.Settings.DidCompleteOnboarding.SetAsync(true);
                }
                else
                {
                    OnboardingPage = OnboardingPage.Page1;
                }
            }
        }

        public async Task ContinueOnboardingAsync()
        {
            if (!await DidCompleteOnboardingAsync())
            {
                switch (OnboardingPage)
                {
                    case OnboardingPage.Page1:
                        OnboardingPage = OnboardingPage.Page2;
                        break;
                    default:
                        await CancelOnboardingAsync();
                        break;
                }
            }
        }

        private async Task CancelOnboardingAsync()
        {
            if (!await DidCompleteOnboardingAsync())
            {
                OnboardingPage = OnboardingPage.None;
                await Services.Settings.DidCompleteOnboarding.SetAsync(true);
            }
        }

        private async ValueTask<bool> DidCompleteOnboardingAsync()
            => await Services.Settings.DidCompleteOnboarding.GetAsync() ?? false;
    }
}
