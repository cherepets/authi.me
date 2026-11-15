using AotServiceProvider;
using Authi.Common.Extensions;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Security.Cryptography;

namespace Authi.Common.Services
{
    [Service]
    public interface ICrypto
    {
        byte[] Encrypt(byte[] bytes, X25519KeyPair key);
        byte[] Decrypt(byte[] bytes, X25519KeyPair key);
        byte[] Encrypt(byte[] bytes, AesKey key);
        byte[] Decrypt(byte[] bytes, AesKey key);
        X25519KeyPair GenerateX25519KeyPair();
        AesKey GenerateAesKey();
    }

    public class Crypto : ICrypto
    {
        public const int AesKeySize = 32;
        public const int AesBlockSize = 16;

        public byte[] Encrypt(byte[] bytes, X25519KeyPair keyPair) => Encrypt(bytes, keyPair.DeriveAesKey());
        public byte[] Decrypt(byte[] bytes, X25519KeyPair keyPair) => Decrypt(bytes, keyPair.DeriveAesKey());

        public byte[] Encrypt(byte[] bytes, AesKey key)
        {
            if (bytes.Length == 0)
            {
                return bytes;
            }

            var iv = RandomNumberGenerator.GetBytes(AesBlockSize);
            var cipher = InitAes(true, key.Bytes.ToArray(), iv);

            var encrypted = cipher.DoFinal(bytes);
            var result = new byte[iv.Length + encrypted.Length];

            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

            return result;
        }

        public byte[] Decrypt(byte[] bytes, AesKey key)
        {
            if (bytes.Length == 0)
            {
                return bytes;
            }

            var iv = new byte[AesBlockSize];
            var ciphertext = new byte[bytes.Length - AesBlockSize];

            Buffer.BlockCopy(bytes, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(bytes, iv.Length, ciphertext, 0, ciphertext.Length);

            var cipher = InitAes(false, key.Bytes.ToArray(), iv);
            return cipher.DoFinal(ciphertext);
        }

        public AesKey GenerateAesKey()
        {
            return new AesKey(RandomNumberGenerator.GetBytes(AesKeySize));
        }

        public X25519KeyPair GenerateX25519KeyPair()
        {
            var generator = new X25519KeyPairGenerator();
            generator.Init(new X25519KeyGenerationParameters(new SecureRandom()));
            var keyPair = generator.GenerateKeyPair();

            var privateKey = ((X25519PrivateKeyParameters)keyPair.Private).GetEncoded();
            var publicKey = ((X25519PublicKeyParameters)keyPair.Public).GetEncoded();

            return new X25519KeyPair(new X25519PrivateKey(privateKey), new X25519PublicKey(publicKey));
        }

        private static PaddedBufferedBlockCipher InitAes(bool forEncryption, byte[] key, byte[] iv)
        {
            var cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesEngine()));
            cipher.Init(forEncryption, new ParametersWithIV(new KeyParameter(key), iv));
            return cipher;
        }
    }

    public class X25519KeyPair(X25519PrivateKey privateKey, X25519PublicKey publicKey)
    {
        public X25519PrivateKey Private { get; } = privateKey;
        public X25519PublicKey Public { get; } = publicKey;

        public AesKey DeriveAesKey()
        {
            var privateKey = new X25519PrivateKeyParameters(Private, 0);
            var publicKey = new X25519PublicKeyParameters(Public, 0);

            var agreement = new X25519Agreement();
            agreement.Init(privateKey);

            var sharedSecret = new byte[Crypto.AesKeySize];
            agreement.CalculateAgreement(publicKey, sharedSecret, 0);

            var hash = SHA256.HashData(sharedSecret);
            return new AesKey(hash);
        }
    }

    public class AesKey(ReadOnlyMemory<byte> bytes) : CryptoKey(bytes)
    {
        protected override int Length => Crypto.AesKeySize;
    }

    public class X25519PrivateKey(ReadOnlyMemory<byte> bytes) : CryptoKey(bytes)
    {
        protected override int Length => Crypto.AesKeySize;
    }

    public class X25519PublicKey(ReadOnlyMemory<byte> bytes) : CryptoKey(bytes)
    {
        protected override int Length => Crypto.AesKeySize;
    }

    public abstract class CryptoKey
    {
        protected abstract int Length { get; }
        public ReadOnlyMemory<byte> Bytes { get; }

        protected CryptoKey(ReadOnlyMemory<byte> bytes)
        {
            Bytes = bytes;
            if (Bytes.Length != Length)
            {
                throw new CryptographicException("Incorrect key size.");
            }
        }

        public static implicit operator ReadOnlyMemory<byte>(CryptoKey key) => key.Bytes;
        public static implicit operator ReadOnlySpan<byte>(CryptoKey key) => key.Bytes.ToArray();
        public static implicit operator byte[](CryptoKey key) => key.Bytes.ToArray();

        public override string ToString()
        {
            return Bytes.ToArray().ToBase64String();
        }
    }
}
