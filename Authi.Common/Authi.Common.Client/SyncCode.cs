using Authi.Common.Extensions;
using Authi.Common.Services;
using System;

namespace Authi.Common.Client
{
    public class SyncCode
    {
        public required Guid SyncId { get; init; }
        public required AesKey DataKey { get; init; }
        public required AesKey OneTimeKey { get; init; }
        public string? ServerUrl { get; init; }

        public byte[] Serialize()
        {
            return
                [
                .. SyncId.ToByteArray(),
                .. DataKey.Bytes.ToArray(),
                .. OneTimeKey.Bytes.ToArray(),
                .. ServerUrl?.ToUtfBytes() ?? []
                ];
        }

        public static SyncCode Deserialize(byte[] bytes)
        {
            const int guidLength = 16;

            var syncId = new Guid(bytes[..guidLength]);

            var offset = guidLength;
            var keySize = Crypto.AesKeySize;

            var dataKeyBytes = bytes[offset..(offset + keySize)];
            offset += keySize;
            var oneTimeKeyBytes = bytes[offset..(offset + keySize)];
            offset += keySize;

            string? serverUrl = null;
            if (offset < bytes.Length)
            {
                serverUrl = bytes[offset..].ToUtfString();
            }

            return new SyncCode
            {
                SyncId = syncId,
                DataKey = new AesKey(dataKeyBytes),
                OneTimeKey = new AesKey(oneTimeKeyBytes),
                ServerUrl = serverUrl
            };
        }
    }
}
