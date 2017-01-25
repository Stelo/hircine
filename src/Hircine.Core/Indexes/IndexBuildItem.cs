using Hircine.VersionedIndex;
using Raven.Client.Indexes;

namespace Hircine.Core.Indexes
{
    public class IndexBuildItem
    {
        public IndexBuildItem() { }
        public IndexBuildItem(AbstractIndexCreationTask definition, IndexVersion version)
        {
            Definition = definition;
            Version = version;
        }

        public AbstractIndexCreationTask Definition { get; set; }

        public IndexVersion Version { get; set; }

    }
}
