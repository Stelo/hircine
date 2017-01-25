using System;

namespace Hircine.VersionedIndex
{
    public class VersionedIndexLog
    {
        public VersionedIndexLog()
        {
            RunOn = DateTimeOffset.UtcNow;
        }

        public VersionedIndexLog(string indexName, IndexVersion version) : this()
        {
            IndexName = indexName;
            Version = version;

            Id = GenerateIdPrefix(indexName) + version;
        }

        public string Id { get; set; }

        public string IndexName { get; set; }
        public IndexVersion Version { get; set; }

        public DateTimeOffset RunOn { get; set; }


        public static string GenerateIdPrefix(string indexName)
        {
            const string entityPrefix = "VersionedIndexLog";
            return $"{entityPrefix}/{indexName}/";
        }
    }
}
