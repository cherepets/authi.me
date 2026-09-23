using Authi.App.Logic.Services;
using Authi.Common.Services;
using System;
using System.Threading.Tasks;
using Windows.Security.Credentials.UI;
using L10n = Authi.App.Logic.Localization;

namespace Authi.App.WinUI.Services
{
    internal class Biometrics : IBiometrics
    {
        public async Task<bool> VerifyAsync()
        {
            try
            {
                if (await UserConsentVerifier.CheckAvailabilityAsync() != UserConsentVerifierAvailability.Available)
                {
                    return false;
                }

                var verificationResult = await UserConsentVerifier.RequestVerificationAsync(L10n.Generic.AppName);

                return verificationResult == UserConsentVerificationResult.Verified;
            }
            catch (Exception exception)
            {
                ServiceProvider.Current.Get<ILogger>().Write(exception);
                return false;
            }
        }
    }
}