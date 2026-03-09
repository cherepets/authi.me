using Authi.Common.Client;

namespace Authi.Common.Test
{
    [TestClass]
    public class OtpathUriTests
    {
        [TestMethod]
        public void TryParseValidUriWithAllParametersReturnsTrue()
        {
            var uri = "otpauth://totp/Issuer:Account?secret=Secret&issuer=Issuer";
            Assert.IsTrue(OtpauthUri.TryParse(uri, out var otpauth));
            Assert.AreEqual("Issuer", otpauth.Issuer);
            Assert.AreEqual("Secret", otpauth.Secret);
            Assert.AreEqual("Account", otpauth.Account);
        }

        [TestMethod]
        public void TryParseValidUriWithNoAccountReturnsTrue()
        {
            var uri = "otpauth://totp/Issuer?secret=Secret";
            Assert.IsTrue(OtpauthUri.TryParse(uri, out var otpauth));
            Assert.AreEqual("Issuer", otpauth.Issuer);
            Assert.AreEqual("Secret", otpauth.Secret);
            Assert.IsNull(otpauth.Account);
        }

        [TestMethod]
        public void TryParseValidUriWithLabelIssuerReturnsTrue()
        {
            var uri = "otpauth://totp/Issuer:Account?secret=Secret";
            Assert.IsTrue(OtpauthUri.TryParse(uri, out var otpauth));
            Assert.AreEqual("Issuer", otpauth.Issuer);
            Assert.AreEqual("Account", otpauth.Account);
            Assert.AreEqual("Secret", otpauth.Secret);
        }

        [TestMethod]
        public void TryParseInvalidUriFormatReturnsFalse()
        {
            Assert.IsFalse(OtpauthUri.TryParse("failure", out _));
            Assert.IsFalse(OtpauthUri.TryParse(string.Empty, out _));
        }

        [TestMethod]
        public void TryParseMissingSecret_ReturnsFalse()
        {
            var uri = "otpauth://totp/Issuer:Account?issuer=Issuer";
            Assert.IsFalse(OtpauthUri.TryParse(uri, out _));
        }

        [TestMethod]
        public void ToStringReturnsCorrectFormat()
        {
            var otpauth = new OtpauthUri("Issuer", "Secret", "Account");
            var expected = "otpauth://totp/Issuer:Account?secret=Secret&issuer=Issuer";
            Assert.AreEqual(expected, otpauth.ToString());
        }

        [TestMethod]
        public void ToStringWithNoAccountReturnsCorrectFormat()
        {
            var otpauth = new OtpauthUri("Issuer", "Secret", null);
            var expected = "otpauth://totp/Issuer:?secret=Secret&issuer=Issuer";
            Assert.AreEqual(expected, otpauth.ToString());
        }
    }
}
