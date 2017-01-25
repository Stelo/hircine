using System;

namespace Hircine.VersionedIndex
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class VersionedIndexAttribute : Attribute
    {
        public VersionedIndexAttribute(int major, int minor, int revision)
        {
            CurrentVersion = new IndexVersion(major, minor, revision);
        }

        public IndexVersion CurrentVersion { get; set; }
    }
}