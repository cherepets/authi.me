using Authi.Common.Services;
using System;

namespace Authi.Common.Client.Results
{
    public class ConsumeResult
    {
        public required Guid ClientId { get; init; }
        public required AesKey DataKey { get; init; }
        public required X25519KeyPair SyncKeyPair { get; init; }
        public string? ServerUrl { get; init; }
    }
}
