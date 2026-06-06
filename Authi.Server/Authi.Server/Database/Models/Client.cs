using System;

namespace Authi.Server.Database.Models
{
    public class Client
    {
        public required Guid ClientId { get; set; }
        public required Guid DataId { get; set; }
        public required KeyPair KeyPair { get; set; }
    }
}
