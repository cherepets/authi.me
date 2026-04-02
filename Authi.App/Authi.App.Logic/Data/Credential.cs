using LiteDB;
using System;

namespace Authi.App.Logic.Data
{
    public class Credential
    {
        [BsonId]
        public ObjectId? LocalId { get; set; }
        public Guid? CloudId { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Secret { get; set; }
        public long? Timestamp { get; set; }

        public bool DataEquals(Credential credential)
        {
            if (Title != credential.Title) return false;
            if (Subtitle != credential.Subtitle) return false;
            if (Secret != credential.Secret) return false;
            if (Timestamp != credential.Timestamp) return false;
            return true;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Credential credential)
            {
                // Do not check LocalId as it's optional
                if (CloudId != credential.CloudId) return false;
                return DataEquals(credential);
            }
            return false;
        }

        public override int GetHashCode() =>
            (LocalId?.GetHashCode() ?? 0) +
            (CloudId?.GetHashCode() ?? 0);
    }
}
