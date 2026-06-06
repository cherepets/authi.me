using Authi.Common.Services;
using System;

namespace Authi.Server.Database.Models
{
    public class KeyPair
    {
        public required string Private { get; set; }
        public required string Public { get; set; }

        public X25519KeyPair ToX25519KeyPair()
        {
            return new X25519KeyPair(
                new X25519PrivateKey(Convert.FromBase64String(Private)),
                new X25519PublicKey(Convert.FromBase64String(Public)));
        }
    }
}
