using System;

namespace Authi.Server.Database.Models
{
    public class Data
    {
        public required Guid DataId { get; set; }
        public required Guid Version { get; set; }
        public required byte[] Binary { get; set; }
        public required long LastAccessedAt { get; set; }
    }
}
