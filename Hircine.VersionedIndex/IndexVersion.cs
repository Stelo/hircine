using System;

namespace Hircine.VersionedIndex
{
    public class IndexVersion : IComparable<IndexVersion>
    {
        public IndexVersion(int major, int minor, int revision)
        {
            Major = major;
            Minor = minor;
            Revision = revision;
        }

        public int Major { get; set; }
        public int Minor { get; set; }
        public int Revision { get; set; }


        public bool IsHigherThan(IndexVersion other)
        {
            return this.CompareTo(other) > 0;
        }

        public int CompareTo(IndexVersion other)
        {
            var major = this.Major.CompareTo(other.Major);
            if (major != 0)
                return major;

            var minor = this.Minor.CompareTo(other.Minor);
            if (minor != 0)
                return minor;

            return this.Revision.CompareTo(other.Revision);
        }

        public override string ToString()
        {
            return $"{Major:D2}.{Minor:D2}.{Revision:D2}";
        }
        
    }
}
