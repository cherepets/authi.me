using Authi.App.Logic.Services;
using Authi.Common.Services;
using Maui.Biometric;
using System;
using System.Threading.Tasks;
using L10n = Authi.App.Logic.Localization;

namespace Authi.App.Maui.Services
{
    internal class Biometrics : IBiometrics
    {
        public async Task<bool> VerifyAsync()
        {
            try
            {
                var result = await BiometricAuthentication.Current.AuthenticateAsync(
                new AuthenticationRequest(
                    title: L10n.Generic.AppName,
                    reason: L10n.Settings.GeneralBiometrics));
                return result.IsSuccessful;
            }
            catch (Exception exception)
            {
                ServiceProvider.Current.Get<ILogger>().Write(exception);
                return false;
            }
        }
    }
}
