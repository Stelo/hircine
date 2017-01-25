using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hircine.Core.Connectivity;
using Hircine.Core.Indexes;
using Hircine.Core.Runtime;
using Hircine.TestIndexes.Indexes;
using NUnit.Framework;
using Raven.Client.Indexes;
using Hircine.VersionedIndex;

namespace Hircine.Core.Tests.Indexes
{
    [TestFixture(Description = "Test fixture used for verifying that we can successfully create indexes using IndexBuilder")]
    public class IndexCreationTests
    {
        private IRavenInstanceFactory _ravenInstanceFactory;
        private Assembly _indexAssembly;

        #region Setup / Teardown

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _ravenInstanceFactory = new DefaultRavenInstanceFactory();
            _indexAssembly = AssemblyRuntimeLoader.LoadAssembly(TestHelper.ValidTestAssemblyPath);
        }

        #endregion

        #region Test Models

        public class SimpleModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Tag { get; set; }
            public DateTimeOffset DateCreated { get; set; }
        }

        public class OtherSimpleModel
        {
            public string Id { get; set; }
            public double Price { get; set; }
            public int NumberOfAlcoholicRabbits { get; set; }
            public string Tag { get; set; }
            public DateTimeOffset DateCreated { get; set; }
        }

        public class TotalDocumentsWithTagPerDay
        {
            public string Tag { get; set; }
            public DateTimeOffset Day { get; set; }
            public int Total { get; set; }
        }

        #endregion

        #region Test Indexes

        //Should be able to create this index, a valid multi-map reduce index
        public class ValidMultiMapReduceIndex : AbstractMultiMapIndexCreationTask<TotalDocumentsWithTagPerDay>
        {
            public ValidMultiMapReduceIndex()
            {
                AddMap<SimpleModel>(models => from model in models
                                                  select new
                                                             {
                                                                 Tag = model.Tag,
                                                                 Day = model.DateCreated.Date,
                                                                 Total = 1
                                                             });

                AddMap<OtherSimpleModel>(models => from model in models
                                                       select new
                                                                  {
                                                                      Tag = model.Tag,
                                                                      Day = model.DateCreated.Date,
                                                                      Total = 1 
                                                                  });

                Reduce = results => from result in results
                                    group result by new {result.Day, result.Tag}
                                    into g
                                    select new
                                               {
                                                   Tag = g.Key.Tag,
                                                   Day = g.Key.Day,
                                                   Total = g.Sum(x => x.Total)
                                               };
            }
        }

        //Should NOT be able to create this index, an INVALID multi-map reduce index
        public class InvalidMultiMapReduceIndex : AbstractMultiMapIndexCreationTask<TotalDocumentsWithTagPerDay>
        {
            public InvalidMultiMapReduceIndex()
            {
                AddMap<SimpleModel>(models => from model in models
                                              select new
                                              {
                                                  Tag = model.Tag,
                                                  Day = model.DateCreated.Date,
                                                  Total = 1
                                              });

                AddMap<OtherSimpleModel>(models => from model in models
                                                   select new
                                                   {
                                                       Tag = model.Tag,
                                                       Day = model.DateCreated.Date,
                                                       Total = 1,
                                                       DrunkAnimals = model.NumberOfAlcoholicRabbits //Can't have an extra field on only one of the mapped documents
                                                   });

                Reduce = results => from result in results
                                    group result by new { result.Day, result.Tag }
                                        into g
                                        select new
                                        {
                                            Tag = g.Key.Tag,
                                            Day = g.Key.Day,
                                            Total = g.Sum(x => x.Total),
                                            DrunkGoats = "LIE" //Extraneous field
                                        };
            }
        }

        #endregion

        #region Tests

        [Test(Description = "Should be able to build our valid RavenDB indexes against an in-memory test database")]
        public void Should_Synchronously_Create_Indexes_Against_Embedded_Database()
        {
            //Assert one pre-condition: must have n > 0 indexes in the assembly before we begin
            var numberOfTargetIndexes = AssemblyRuntimeLoader.GetRavenDbIndexes(_indexAssembly).Count;
            Assert.IsTrue(numberOfTargetIndexes > 0, "Pre-condition failed: must have at least 1 index in the defined assembly");

            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();
            
            var indexBuilder = new IndexBuilder(embeddedDb, _indexAssembly);
            try
            {
                var indexBuildResults = indexBuilder.Run(new IndexBuildCommand(), null);
                Assert.IsNotNull(indexBuildResults);
                Assert.IsTrue(indexBuildResults.Created > 0, "Should have been able to successfully build at least 1 index");
                Assert.AreEqual(numberOfTargetIndexes, indexBuildResults.Created, "Expected the number of built indexes to match the number of indexes defined in the assembly");
                Assert.IsTrue(indexBuildResults.Cancelled == 0, "Should not have had any index building jobs cancelled");
                Assert.IsTrue(indexBuildResults.Failed == 0, "Should not have had any index building jobs fail");
                Assert.IsTrue(indexBuildResults.Deleted == 0, "Should not have deleted any indexes");
            }
            catch (InvalidOperationException ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                indexBuilder.Dispose();
            }
        }

        [Test(Description = "We should be able to receive a failure notification back from RavenDb when we try to create an index that is invalid")]
        public void Should_Report_IndexCreationFailure_When_Building_Invalid_Index()
        {
            var invalidMultiMapIndex = new InvalidMultiMapReduceIndex();

            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();

            var indexBuilder = new IndexBuilder(embeddedDb, _indexAssembly);

            try
            {

                var indexBuildResult = indexBuilder.BuildIndex(invalidMultiMapIndex);

                Assert.IsNotNull(indexBuildResult);
                Assert.AreEqual(invalidMultiMapIndex.IndexName, indexBuildResult.IndexName);
                Assert.AreEqual(BuildResult.Failed, indexBuildResult.Result);
            }
            catch (InvalidOperationException ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                indexBuilder.Dispose();
            }
        }

        [Test(Description = "We should recieve a success notification when we are able to successfully build an index against RavenDB")]
        public void Should_Report_IndexCreationSuccess_When_Building_Valid_Index()
        {
            var validMultiMapIndex = new ValidMultiMapReduceIndex();

            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();

            var indexBuilder = new IndexBuilder(embeddedDb, _indexAssembly);

            try
            {
                var indexBuildResult = indexBuilder.BuildIndex(validMultiMapIndex);

                Assert.IsNotNull(indexBuildResult);
                Assert.AreEqual(validMultiMapIndex.IndexName, indexBuildResult.IndexName);
                Assert.AreEqual(BuildResult.Created, indexBuildResult.Result);
            }
            catch (InvalidOperationException ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                indexBuilder.Dispose();
            }
        }

        [Test(Description = "Versioning isn't explicitly required; unversioned indexes should still be created but should not generate VersionedIndexLogs")]
        public void Unversioned_Index_Creation_Does_Not_Create_VersionIndexLog()
        {
            var unversionedIndex = new BlogPostsByAuthor();
            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();

            var indexBuilder = new IndexBuilder(embeddedDb, _indexAssembly);

            try
            {
                var indexBuildResult = indexBuilder.BuildIndex(unversionedIndex);

                Assert.IsNotNull(indexBuildResult);
                Assert.AreEqual(unversionedIndex.IndexName, indexBuildResult.IndexName);
                Assert.AreEqual(BuildResult.Created, indexBuildResult.Result);

                using (var session = embeddedDb.OpenSession())
                {
                    var logs = session.Query<VersionedIndexLog>();
                    Assert.IsEmpty(logs);
                }
            }
            catch (InvalidOperationException ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                indexBuilder.Dispose();
            }

        }

        [Test, Description("Should generate logs for each cumulate update to the same index.")]
        public void Versioned_Index_Creation_Creates_VersionIndexLog_For_Each_Update()
        {
            var versionedIndex = new BlogPostsByAuthorVersion100();
            var versionedIndex2 = new BlogPostsByAuthorVersion110();

            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();

            var indexBuilder = new IndexBuilder(embeddedDb, _indexAssembly);

            try
            {
                var indexBuildResult = indexBuilder.BuildIndex(versionedIndex);
                Assert.IsNotNull(indexBuildResult);
                Assert.AreEqual(versionedIndex.IndexName, indexBuildResult.IndexName);
                Assert.AreEqual(BuildResult.Created, indexBuildResult.Result);

                using (var session = embeddedDb.OpenSession())
                {
                    var logs = session.Query<VersionedIndexLog>().ToList();
                    Assert.AreEqual(1, logs.Count);

                    Assert.AreEqual(versionedIndex.IndexName, logs[0].IndexName);

                    Assert.AreEqual(1, logs[0].Version.Major);
                    Assert.AreEqual(0, logs[0].Version.Minor);
                    Assert.AreEqual(0, logs[0].Version.Revision);
                }

                var indexBuildResult2 = indexBuilder.BuildIndex(versionedIndex2);
                Assert.IsNotNull(indexBuildResult2);
                Assert.AreEqual(versionedIndex2.IndexName, indexBuildResult2.IndexName);
                Assert.AreEqual(BuildResult.Created, indexBuildResult2.Result);

                using (var session = embeddedDb.OpenSession())
                {
                    var logs = session.Query<VersionedIndexLog>().ToList();
                    Assert.AreEqual(2, logs.Count());

                    var logId = VersionedIndexLog.GenerateIdPrefix(versionedIndex2.IndexName) + "01.01.00";
                    var newLog = session.Load<VersionedIndexLog>(logId);

                    Assert.AreEqual(versionedIndex.IndexName, newLog.IndexName);

                    Assert.AreEqual(1, newLog.Version.Major);
                    Assert.AreEqual(1, newLog.Version.Minor);
                    Assert.AreEqual(0, newLog.Version.Revision);
                }

            }
            catch (InvalidOperationException ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                indexBuilder.Dispose();
            }
        }

        [Test, Description("Should not execute index change when version is the same.")]
        public void Versioned_Index_Is_Not_Created_When_Log_Exists_For_Same_Version()
        {
            var versionedIndex = new BlogPostsByAuthorVersion100();
            var sameVersionIndex = new BlogPostsByAuthorVersion100_DefinitionChangeWithoutVersionChange();

            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();

            var indexBuilder = new IndexBuilder(embeddedDb, _indexAssembly);

            try
            {
                var indexBuildResult = indexBuilder.BuildIndex(versionedIndex);
                Assert.IsNotNull(indexBuildResult);
                Assert.AreEqual(versionedIndex.IndexName, indexBuildResult.IndexName);
                Assert.AreEqual(BuildResult.Created, indexBuildResult.Result);


                var indexBuildResult2 = indexBuilder.BuildIndex(sameVersionIndex);
                Assert.IsNotNull(indexBuildResult2);
                Assert.AreEqual(BuildResult.VersionCheckFailed, indexBuildResult2.Result);

                using (var session = embeddedDb.OpenSession())
                {
                    var logs = session.Query<VersionedIndexLog>().ToList();
                    Assert.AreEqual(1, logs.Count);

                    Assert.AreEqual(versionedIndex.IndexName, logs[0].IndexName);

                    Assert.AreEqual(1, logs[0].Version.Major);
                    Assert.AreEqual(0, logs[0].Version.Minor);
                    Assert.AreEqual(0, logs[0].Version.Revision);
                }

            }
            catch (InvalidOperationException ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                indexBuilder.Dispose();
            }
        }

        [Test(Description = "Should be able to report progress on batch jobs via the callback we pass in without any issues")]
        public void Should_Report_Progress_on_Batch_Jobs()
        {
            //Assert one pre-condition: must have n > 0 indexes in the assembly before we begin
            var numberOfTargetIndexes = AssemblyRuntimeLoader.GetRavenDbIndexes(_indexAssembly).Count;
            Assert.IsTrue(numberOfTargetIndexes > 0, "Pre-condition failed: must have at least 1 index in the defined assembly");

            var embeddedDb = _ravenInstanceFactory.GetEmbeddedInstance(runInMemory: true);
            embeddedDb.Initialize();

            var indexV110LogId = VersionedIndexLog.GenerateIdPrefix("BlogPostsByAuthor") + "01.01.00";

            // Mock creation log of v1.0.0 of BlogPostsByAuthor index so it gets skipped
            using (var session = embeddedDb.OpenSession())
            {
                numberOfTargetIndexes = numberOfTargetIndexes - 2;
                session.Store(new VersionedIndexLog("BlogPostsByAuthor", new IndexVersion(1,0,0)));
                session.SaveChanges();

                var shouldntExistYet = session.Load<VersionedIndexLog>(indexV110LogId);
                Assert.IsNull(shouldntExistYet);
            }

            var listBuildResults = new List<IndexBuildResult>();
            var indexBuilder = new IndexBuilder(embeddedDb, _indexAssembly);
            try
            {
                var indexBuildResults = indexBuilder.Run(new IndexBuildCommand(), x =>
                {
                    //Add the results to the list as the test runs
                    listBuildResults.Add(x);
                });

                //Assert that the job was valid first
                Assert.IsNotNull(indexBuildResults);
                Assert.IsTrue(indexBuildResults.Created > 0, "Should have been able to successfully build at least 1 index");
                Assert.AreEqual(numberOfTargetIndexes, indexBuildResults.Created, "Expected the number of built indexes to match the number of indexes defined in the assembly");
                Assert.IsTrue(indexBuildResults.Cancelled == 0, "Should not have had any index building jobs cancelled");
                Assert.IsTrue(indexBuildResults.Failed == 0, "Should not have had any index building jobs fail");
                Assert.IsTrue(indexBuildResults.Deleted == 0, "Should not have deleted any indexes");
                Assert.IsTrue(indexBuildResults.VersionCheckFailed == 2, "Should have failed version check on one index");

                //Now assert that progress was reported correctly and completely
                Assert.AreEqual(numberOfTargetIndexes, listBuildResults.Count , "Expected the number of calls against the progress method to be equal to the number of indexes in the assembly");
                Assert.AreEqual(indexBuildResults.Created, listBuildResults.Count, "Expected the number of calls against the progress method to be equal to the number of valid indexes built from the assembly, which should be ALL of them in this case");

                using (var session = embeddedDb.OpenSession())
                {
                    var newLog = session.Load<VersionedIndexLog>(indexV110LogId);

                    Assert.AreEqual("BlogPostsByAuthor", newLog.IndexName);

                    Assert.AreEqual(1, newLog.Version.Major);
                    Assert.AreEqual(1, newLog.Version.Minor);
                    Assert.AreEqual(0, newLog.Version.Revision);
                }
            }
            catch (InvalidOperationException ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                indexBuilder.Dispose();
            }
        }

        [Test(Description = "Should be able to produce a list of indexes gathered from all of the assemblies specified in the IndexBuilder constructor")]
        public void Should_Find_Indexes_From_All_Assemblies()
        {
            //We know that our current assembly ALSO has indexes
            var currentAssembly = Assembly.GetExecutingAssembly();

            //Assert that the current assembly has at least one index definied in it
            Assert.IsTrue(AssemblyRuntimeLoader.HasRavenDbIndexes(currentAssembly));
            Assert.IsTrue(AssemblyRuntimeLoader.HasRavenDbIndexes(_indexAssembly));

            var totalIndexes = AssemblyRuntimeLoader.GetRavenDbIndexes(currentAssembly).Count +
                               AssemblyRuntimeLoader.GetRavenDbIndexes(_indexAssembly).Count;

            Assert.IsTrue(totalIndexes > 1, "Should have at least 2 indexes defined between the two assemblies");

            //Hand off the index DBs to the IndexBuilder
            var indexBuilder = new IndexBuilder(null, new[] {currentAssembly, _indexAssembly});

            //See how many indexes the assembly builder finds
            var indexes = indexBuilder.GetIndexesFromLoadedAssemblies();
            Assert.AreEqual(totalIndexes, indexes.Count);
        }

        #endregion
    }
}
