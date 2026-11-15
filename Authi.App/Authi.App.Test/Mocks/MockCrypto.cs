using Authi.Common.Extensions;
using Authi.Common.Services;
using System.Collections.Generic;
using System.Linq;

namespace Authi.App.Test.Mocks
{
    internal class MockCrypto(Dictionary<string, string> encrypt) : ICrypto
    {
        public MockCrypto() : this([])
        {
        }

        private readonly Dictionary<string, string> _encrypt = encrypt;
        private readonly Dictionary<string, string> _decrypt = encrypt
            .ToDictionary(
                kvp => kvp.Value,
                kvp => kvp.Key);

        public byte[] Decrypt(byte[] bytes, X25519KeyPair key)
        {
            return _decrypt[bytes.ToUtfString()].ToUtfBytes();
        }

        public byte[] Decrypt(byte[] bytes, AesKey key)
        {
            return _decrypt[bytes.ToUtfString()].ToUtfBytes();
        }

        public byte[] Encrypt(byte[] bytes, X25519KeyPair key)
        {
            return _encrypt[bytes.ToUtfString()].ToUtfBytes();
        }

        public byte[] Encrypt(byte[] bytes, AesKey key)
        {
            return _encrypt[bytes.ToUtfString()].ToUtfBytes();
        }

        public AesKey GenerateAesKey()
        {
            return new Crypto().GenerateAesKey();
        }

        public X25519KeyPair GenerateX25519KeyPair()
        {
            return new Crypto().GenerateX25519KeyPair();
        }
    }
}
