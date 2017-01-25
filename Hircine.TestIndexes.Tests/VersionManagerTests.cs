using System.Reflection;
using Hircine.Core.Connectivity;
using NUnit.Framework;

namespace Hircine.TestIndexes.Tests
{
    [TestFixture]
    public class VersionManagerTests
    {
        private IRavenInstanceFactory _ravenInstanceFactory;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _ravenInstanceFactory = new DefaultRavenInstanceFactory();
        }


        [Test]
        public void GetIndexVersion_Should_Correctly_Return_Version_When_Defined()
        {
            
        }
    
    }
}
