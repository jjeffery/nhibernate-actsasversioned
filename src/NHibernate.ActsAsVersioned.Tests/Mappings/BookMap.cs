using FluentNHibernate.Mapping;
using NHibernate.ActsAsVersioned.Models;

namespace NHibernate.ActsAsVersioned.Mappings
{
    public class BookMap : ClassMap<Book>
    {
        public BookMap()
        {
            Table("books");
            Id(x => x.Id).Column("id");
            Map(x => x.Title)
                .Column("title")
                .Not.Nullable();
            References(x => x.Author)
                .Column("author_id")
                .Not.Nullable();
        }
    }
}
