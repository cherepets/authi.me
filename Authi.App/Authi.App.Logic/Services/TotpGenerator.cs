using AotServiceProvider;
using Authi.App.Logic.Extensions;
using OtpNet;
using System;

namespace Authi.App.Logic.Services
{
    [Service]
    internal interface ITotpGenerator
    {
        bool TryCalculateTotp(string secret, out string? totp);
        int GetRemainingMs();
    }

    public class TotpGenerator : ServiceBase, ITotpGenerator
    {
        public bool TryCalculateTotp(string secret, out string? totp)
        {
            try
            {
                var bytes = secret.ToBase32Bytes();
                totp = new Totp(bytes).ComputeTotp(Services.Clock.UniversalTime.DateTime);
                return true;
            }
            catch (Exception exception)
            {
                Services.Logger.Write(exception);
                totp = null;
                return false;
            }
        }

        public int GetRemainingMs()
        {
            return Config.UpdateMs - (int)(Services.Clock.UniversalTime.ToUnixTimeMilliseconds() % Config.UpdateMs);
        }
    }
}
