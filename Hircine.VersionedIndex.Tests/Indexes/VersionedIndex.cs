using System.Linq;
using Hircine.VersionedIndex.Tests.Models;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Hircine.VersionedIndex.Tests.Indexes
{

    [VersionedIndex(1, 0, 0)]
    public class VersionedIndexV1 : AbstractIndexCreationTask<TestModel>
    {
        public override string IndexName { get { return "VersionedIndex"; } }

        public VersionedIndexV1()
        {
            Map = things => from thing in things
                            select new
                            {
                                thing.Number,
                                thing.Text,
                                thing.Created
                            };

            Sort(x => x.Created, SortOptions.Custom);
        }
    }

    [VersionedIndex(1, 1, 0)]
    public class VersionedIndexV2 : AbstractIndexCreationTask<TestModel>
    {
        public override string IndexName { get { return "VersionedIndex"; } }

        public VersionedIndexV2()
        {
            Map = things => from thing in things
                            select new
                            {
                                thing.Number,
                                thing.Text,
                                thing.Created,
                                ThingCount = thing.AListOfThings.Count
                            };

            Sort(x => x.Created, SortOptions.Custom);
        }
    }
}
