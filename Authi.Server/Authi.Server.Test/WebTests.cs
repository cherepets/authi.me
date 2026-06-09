using Authi.Common.Services;
using Authi.Common.Test.Mocks;
using Authi.Server.Services;
using Authi.Server.Test.Mocks;
using Microsoft.AspNetCore.Http;
using OtpNet;
using System;

namespace Authi.Server.Test
{
    [TestClass]
    public class WebTests : ServerTestsBase
    {
        private const string CredentialHash = "credential-hash";
        private const string TotpSecret = "JBSWY3DPEHPK3PXP";
        private readonly DateTimeOffset _now = new(2026, 6, 8, 12, 0, 0, TimeSpan.Zero);

        [TestMethod]
        public void ValidCookieIsAuthorizedTest()
        {
            var timestamp = _now.ToUnixTimeMilliseconds();

            Assert.IsTrue(IsAuthorized(CreateCookie(CredentialHash, timestamp, TotpSecret)));
        }

        [TestMethod]
        public void CredentialHashMustMatchTest()
        {
            var timestamp = _now.ToUnixTimeMilliseconds();

            Assert.IsFalse(IsAuthorized(CreateCookie("wrong-hash", timestamp, TotpSecret)));
        }

        [TestMethod]
        public void ExpiredTimestampIsRejectedTest()
        {
            var timestamp = _now.AddHours(-1).AddMilliseconds(-1).ToUnixTimeMilliseconds();

            Assert.IsFalse(IsAuthorized(CreateCookie(CredentialHash, timestamp, TotpSecret)));
        }

        [TestMethod]
        public void FutureTimestampWithinWindowIsAcceptedTest()
        {
            var timestamp = _now.AddMilliseconds(1).ToUnixTimeMilliseconds();

            Assert.IsTrue(IsAuthorized(CreateCookie(CredentialHash, timestamp, TotpSecret)));
        }

        [TestMethod]
        public void TotpMustMatchTimestampTest()
        {
            var timestamp = _now.ToUnixTimeMilliseconds();
            var cookie = $"{CredentialHash}:{timestamp}:000000";

            Assert.IsFalse(IsAuthorized(cookie));
        }

        [TestMethod]
        public void MalformedCookieIsRejectedTest()
        {
            Assert.IsFalse(IsAuthorized("invalid"));
        }

        [TestMethod]
        public void InvalidTotpSecretIsRejectedTest()
        {
            var timestamp = _now.ToUnixTimeMilliseconds();
            var cookie = CreateCookie(CredentialHash, timestamp, TotpSecret);

            Assert.IsFalse(IsAuthorized(cookie, "invalid!"));
        }

        private bool IsAuthorized(string cookie, string configuredSecret = TotpSecret)
        {
            ServicesMock.Override<IClock>(new MockClock { UniversalTime = _now });
            ServicesMock.Override<IConfiguration>(
                new MockConfiguration(CredentialHash, configuredSecret));

            var context = new DefaultHttpContext();
            context.Request.Headers.Cookie = $"auth={Uri.EscapeDataString(cookie)}";
            return new MockWebPage().IsAuthorized(context);
        }

        private static string CreateCookie(string credentialHash, long timestamp, string secret)
        {
            var time = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
            var totp = new Totp(Base32Encoding.ToBytes(secret))
                .ComputeTotp(time);
            return $"{credentialHash}:{timestamp}:{totp}";
        }

    }
}
