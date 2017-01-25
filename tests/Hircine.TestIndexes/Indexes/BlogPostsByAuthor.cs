using System.Linq;
using Hircine.TestIndexes.Models;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using Hircine.VersionedIndex;

namespace Hircine.TestIndexes.Indexes
{
    public class BlogPostsByAuthor : AbstractIndexCreationTask<BlogPost>
    {
        public BlogPostsByAuthor()
        {
            Map = posts => from post in posts
                           select new
                           {
                               post.Author.Id,
                               post.Author.Name
                           };

            Index(x => x.Author.Name, FieldIndexing.Default);
        }
    }

    [VersionedIndex(1, 0, 0)]
    public class BlogPostsByAuthorVersion100 : AbstractIndexCreationTask<BlogPost>
    {
        public override string IndexName { get { return "BlogPostsByAuthor"; } }

        public BlogPostsByAuthorVersion100()
        {
            Map = posts => from post in posts
                           select new
                           {
                               post.Author.Id,
                               post.Author.Name,
                               post.TimePosted
                           };

            Index(x => x.Author.Name, FieldIndexing.Default);
            Sort(x => x.TimePosted, SortOptions.Custom);
        }
    }

    [VersionedIndex(1, 0, 0)]
    public class BlogPostsByAuthorVersion100_DefinitionChangeWithoutVersionChange : AbstractIndexCreationTask<BlogPost>
    {
        public override string IndexName { get { return "BlogPostsByAuthor"; } }

        public BlogPostsByAuthorVersion100_DefinitionChangeWithoutVersionChange()
        {
            Map = posts => from post in posts
                           select new
                           {
                               post.Author.Id,
                               post.Author.Name,
                               post.TimePosted,
                               Foo = 123
                           };

            Index(x => x.Author.Name, FieldIndexing.Default);
            Sort(x => x.TimePosted, SortOptions.Custom);
        }
    }

    [VersionedIndex(1, 1, 0)]
    public class BlogPostsByAuthorVersion110 : AbstractIndexCreationTask<BlogPost>
    {
        public override string IndexName { get { return "BlogPostsByAuthor"; } }

        public BlogPostsByAuthorVersion110()
        {
            Map = posts => from post in posts
                           select new
                           {
                               post.Author.Id,
                               post.Author.Name,
                               post.TimePosted,
                               CommentCount = post.Comments.Count
                           };

            Index(x => x.Author.Name, FieldIndexing.Default);
            Sort(x => x.TimePosted, SortOptions.Custom);
        }
    }
}
