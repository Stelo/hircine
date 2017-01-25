using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;

namespace Hircine.VersionedIndex
{
    public class VersionManager
    {
        private readonly IDocumentStore _documentStore;

        public VersionManager(IDocumentStore documentStore)
        {
            _documentStore = documentStore;
        }

        /// <summary>
        /// Identify the version of the specified Type 
        /// </summary>
        /// <param name="indexType">An index type inheriting AbstractIndexCreationTask</param>
        /// <returns>The current version defined by the VersionIndexAttribute if present. Otherwise, null.</returns>
        public static IndexVersion GetIndexVersion(Type indexType)
        {
            var versionAttribute = indexType.GetCustomAttributes(typeof(VersionedIndexAttribute), false).SingleOrDefault() as VersionedIndexAttribute;
            return versionAttribute?.CurrentVersion;
        }

        /// <summary>
        /// Determines if the local version of the index is higher than the remote one
        /// </summary>
        /// <param name="localVersion">The version of the index defined locally</param>
        /// <param name="indexName">The name of the index to check</param>
        /// <returns>
        /// True, if the local index version supercedes the remote version, or if either the remote or the local index is 
        /// unversioned. Otherwise, false.
        /// </returns>
        public bool IsHigherVersion(IndexVersion localVersion, string indexName)
        {
            if (localVersion == null)
                return true;

            List<VersionedIndexLog> logs;
            using (var session = _documentStore.OpenSession())
            {
                var idPrefix = VersionedIndexLog.GenerateIdPrefix(indexName);
                logs = session.Advanced.LoadStartingWith<VersionedIndexLog>(idPrefix)?.ToList();
            }

            if (logs == null)
                return true;

            var remoteVersions = logs.Where(x => x.IndexName.Equals(indexName)).ToList();
            if (remoteVersions.Any())
            {
                var highestVersion = remoteVersions.OrderByDescending(x => x.Version).First();
                return localVersion.IsHigherThan(highestVersion.Version);
            }

            return true;
        }

        /// <summary>
        /// Insert a VersionedIndexLog for a successfully created index
        /// </summary>
        /// <param name="indexName">The name of the index that was updated</param>
        /// <param name="toVersion">The version of the updated index</param>
        public void LogUpdate(string indexName, IndexVersion toVersion)
        {
            if (toVersion == null)
                return;

            using (var session = _documentStore.OpenSession())
            {
                var log = new VersionedIndexLog(indexName, toVersion);
                session.Store(log);
                session.SaveChanges();
            }
        }
    }
}
