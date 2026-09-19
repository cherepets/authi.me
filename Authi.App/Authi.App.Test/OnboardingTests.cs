using Authi.App.Logic.Data;
using Authi.App.Logic.Services;
using Authi.App.Logic.ViewModels;
using Authi.App.Test.Mocks;
using System.Threading.Tasks;

using static Authi.App.Test.Mocks.MockCredentialStorage;

namespace Authi.App.Test
{
    [TestClass]
    public class OnboardingTests : AppTestsBase
    {
        [TestMethod]
        public async Task CompleteOnboardingTest()
        {
            ServicesMock
                .Override<ILocalCredentialStorage>(new MockCredentialStorage([]))
                .Override<ISettings>(new MockSettings());

            Assert.IsFalse(await DidCompleteOnboardingAsync());

            var mainVM = new MainPageViewModel();
            var onboardingVM = mainVM.OnboardingViewModel;
            await mainVM.InitializeAsync();
            Assert.AreEqual(OnboardingPage.Page1, onboardingVM.OnboardingPage);
            Assert.IsFalse(await DidCompleteOnboardingAsync());

            await onboardingVM.ContinueOnboardingAsync();
            Assert.AreEqual(OnboardingPage.Page2, onboardingVM.OnboardingPage);
            Assert.IsFalse(await DidCompleteOnboardingAsync());

            await onboardingVM.ContinueOnboardingAsync();
            Assert.AreEqual(OnboardingPage.None, onboardingVM.OnboardingPage);
            Assert.IsTrue(await DidCompleteOnboardingAsync());
        }

        [TestMethod]
        public async Task NoOnboardingForExistingUsersTest()
        {
            ServicesMock
                .Override<ILocalCredentialStorage>(
                    new MockCredentialStorage([
                        new Credential
                        {
                            LocalId = CreateLocalId(1),
                            Title = string.Empty,
                            Secret = string.Empty,
                            Timestamp = 1
                        }
                    ]))
                .Override<ISettings>(new MockSettings());

            Assert.IsFalse(await DidCompleteOnboardingAsync());

            var mainVM = new MainPageViewModel();
            var onboardingVM = mainVM.OnboardingViewModel;
            await mainVM.InitializeAsync();
            Assert.AreEqual(OnboardingPage.None, onboardingVM.OnboardingPage);
            Assert.IsTrue(await DidCompleteOnboardingAsync());
        }

        private async ValueTask<bool> DidCompleteOnboardingAsync()
            => await Services.Settings.DidCompleteOnboarding.GetAsync() ?? false;
    }
}