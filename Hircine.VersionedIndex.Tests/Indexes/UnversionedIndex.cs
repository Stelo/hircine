using System.Linq;
using Hircine.VersionedIndex.Tests.Models;
using Raven.Client.Indexes;

namespace Hircine.VersionedIndex.Tests.Indexes
{

    public class UnversionedIndex : AbstractIndexCreationTask<TestModel>
    {
        public UnversionedIndex()
        {
            Map = things => from thing in things
                select new
                {
                    thing.Number,
                    thing.Text
                };
        }
    }
}
