using System;

namespace Authi.Common.Client
{
    public class CredentialDto
    {
        public required Guid CloudId { get; init; }
        public required string Title { get; init; }
        public required string? Subtitle { get; init; }
        public required string Secret { get; init; }
        public required long Timestamp { get; init; }
    }
}
