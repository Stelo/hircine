using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Indexes;

namespace Hircine.VersionedIndex.Tests.Models
{
    public class TestModel
    {
        public int Number { get; set; }
        public string Text { get; set; }
        public List<string> AListOfThings { get; set; }

        public DateTime Created { get; set; }
    }
}
