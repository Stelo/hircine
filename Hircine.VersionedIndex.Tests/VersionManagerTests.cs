using Hircine.Core.Connectivity;
using Hircine.VersionedIndex.Tests.Indexes;
using NUnit.Framework;

namespace Hircine.VersionedIndex.Tests
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
            var version = VersionManager.GetIndexVersion(typeof(VersionedIndexV1));
            Assert.NotNull(version);
            Assert.AreEqual(1, version.Major);
            Assert.AreEqual(0, version.Minor);
            Assert.AreEqual(0, version.Revision);
        }


        [Test]
        public void GetIndexVersion_Should_Return_Null_When_No_Version_Defined()
        {
            var version = VersionManager.GetIndexVersion(typeof(UnversionedIndex));
            Assert.IsNull(version);
        }

        [Test]
        public void IsHigherVersion_Should_Return_True_When_No_Version_Defined()
        {
            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();

            var versionManager = new VersionManager(embeddedDb);
            Assert.IsTrue(versionManager.IsHigherVersion(null, "NotDefined"));
        }

        [Test]
        public void IsHigherVersion_Should_Return_True_When_No_Previous_Version_Logged()
        {
            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();

            var versionManager = new VersionManager(embeddedDb);
            Assert.IsTrue(versionManager.IsHigherVersion(new IndexVersion(0,0,0), "NotDefined"));
        }

        [Test]
        public void IsHigherVersion_Should_Return_False_When_Previous_Version_Is_Same()
        {
            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();
            using (var session = embeddedDb.OpenSession())
            {
                session.Store(new VersionedIndexLog("IndexWithVersion100", new IndexVersion(1, 0, 0)));
                session.SaveChanges();
            }

            var versionManager = new VersionManager(embeddedDb);
            Assert.IsFalse(versionManager.IsHigherVersion(new IndexVersion(1,0,0), "IndexWithVersion100"));
        }


        [Test]
        public void IsHigherVersion_Should_Return_False_When_Previous_Revision_Is_Higher()
        {
            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();
            using (var session = embeddedDb.OpenSession())
            {
                session.Store(new VersionedIndexLog("IndexWithVersion101", new IndexVersion(1, 0, 1)));
                session.SaveChanges();
            }

            var versionManager = new VersionManager(embeddedDb);
            Assert.IsFalse(versionManager.IsHigherVersion(new IndexVersion(1, 0, 0), "IndexWithVersion101"));
        }

        [Test]
        public void IsHigherVersion_Should_Return_False_When_Previous_Minor_Is_Higher()
        {
            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();
            using (var session = embeddedDb.OpenSession())
            {
                session.Store(new VersionedIndexLog("IndexWithVersion110", new IndexVersion(1, 1, 0)));
                session.SaveChanges();
            }

            var versionManager = new VersionManager(embeddedDb);
            Assert.IsFalse(versionManager.IsHigherVersion(new IndexVersion(1, 0, 0), "IndexWithVersion110"));
        }

        [Test]
        public void IsHigherVersion_Should_Return_False_When_Previous_Major_Is_Higher()
        {
            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();
            using (var session = embeddedDb.OpenSession())
            {
                session.Store(new VersionedIndexLog("IndexWithVersion200", new IndexVersion(2, 0, 0)));
                session.SaveChanges();
            }

            var versionManager = new VersionManager(embeddedDb);
            Assert.IsFalse(versionManager.IsHigherVersion(new IndexVersion(1, 0, 0), "IndexWithVersion200"));
        }

        [Test]
        public void IsHigherVersion_Should_Return_True_When_Previous_Revision_Is_Lower()
        {
            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();
            using (var session = embeddedDb.OpenSession())
            {
                session.Store(new VersionedIndexLog("IndexWithVersion101", new IndexVersion(1, 0, 1)));
                session.SaveChanges();
            }

            var versionManager = new VersionManager(embeddedDb);
            Assert.IsTrue(versionManager.IsHigherVersion(new IndexVersion(1, 0, 2), "IndexWithVersion101"));
        }

        [Test]
        public void IsHigherVersion_Should_Return_True_When_Previous_Minor_Is_Lower()
        {
            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();
            using (var session = embeddedDb.OpenSession())
            {
                session.Store(new VersionedIndexLog("IndexWithVersion110", new IndexVersion(1, 1, 0)));
                session.SaveChanges();
            }

            var versionManager = new VersionManager(embeddedDb);
            Assert.IsTrue(versionManager.IsHigherVersion(new IndexVersion(1, 2, 0), "IndexWithVersion110"));
        }

        [Test]
        public void IsHigherVersion_Should_Return_True_When_Previous_Major_Is_Lower()
        {
            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();
            using (var session = embeddedDb.OpenSession())
            {
                session.Store(new VersionedIndexLog("IndexWithVersion200", new IndexVersion(2, 0, 0)));
                session.SaveChanges();
            }

            var versionManager = new VersionManager(embeddedDb);
            Assert.IsTrue(versionManager.IsHigherVersion(new IndexVersion(3, 0, 0), "IndexWithVersion200"));
        }

    }
}
