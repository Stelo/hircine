using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hircine.VersionedIndex
{
    public interface IVersionedIndex
    {
        string GetIndexNameWithoutVersion();
    }

    class VersionIndexName
    {
        public static string Create(string indexName, IndexDefinition indexDefinition)
        {
            return indexName + "-" + Convert.ToBase64String(indexDefinition.GetIndexHash());
        }
    }

    public abstract class AbstractMultiMapVersionedIndexCreationTask<TReduceResult> : AbstractMultiMapIndexCreationTask<TReduceResult>, IVersionedIndex
    {
        string _indexName;

        public override string IndexName
        {
            get
            {
                if (string.IsNullOrEmpty(_indexName))
                    _indexName = VersionIndexName.Create(base.IndexName, CreateIndexDefinition());

                return _indexName;
            }
        }

        public string GetIndexNameWithoutVersion() { return base.IndexName; } 
    }

    public abstract class AbstractMultiMapVersionedIndexCreationTask : AbstractMultiMapIndexCreationTask, IVersionedIndex
    {
        string _indexName;

        public override string IndexName
        {
            get
            {
                if (string.IsNullOrEmpty(_indexName))
                    _indexName = VersionIndexName.Create(base.IndexName, CreateIndexDefinition());

                return _indexName;
            }
        }

        public string GetIndexNameWithoutVersion() { return base.IndexName; }
    }

    public abstract class AbstractVersionedIndexCreationTask<TDocument> : AbstractIndexCreationTask<TDocument>, IVersionedIndex
    {
        string _indexName;

        public override string IndexName
        {
            get
            {
                if (string.IsNullOrEmpty(_indexName))
                    _indexName = VersionIndexName.Create(base.IndexName, CreateIndexDefinition());

                return _indexName;
            }
        }
        public string GetIndexNameWithoutVersion() { return base.IndexName; } 
    }

    public abstract class AbstractVersionedIndexCreationTask<TDocument, TReduceResult> : AbstractIndexCreationTask<TDocument, TReduceResult>, IVersionedIndex
    {
        string _indexName;

        public override string IndexName
        {
            get
            {
                if (string.IsNullOrEmpty(_indexName))
                    _indexName = VersionIndexName.Create(base.IndexName, CreateIndexDefinition());

                return _indexName;
            }
        }

        public string GetIndexNameWithoutVersion() { return base.IndexName; }
    }
}
