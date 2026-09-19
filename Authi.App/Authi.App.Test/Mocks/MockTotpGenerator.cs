using Authi.App.Logic.Services;
using System.Collections.Generic;

namespace Authi.App.Test.Mocks
{
    internal class MockTotpGenerator(Dictionary<string, string> _codes) : ITotpGenerator
    {
        public MockTotpGenerator() : this([])
        {
        }

        public int GetRemainingMs() => 30000;

        public bool TryCalculateTotp(string secret, out string? totp)
        {
            return _codes.TryGetValue(secret, out totp);
        }
    }
}
