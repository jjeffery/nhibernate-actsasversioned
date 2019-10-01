using FluentNHibernate.Mapping;
using NHibernate.ActsAsVersioned.Models;
using NHibernate.Type;
using CustomType = NHibernate.ActsAsVersioned.Models.CustomType;

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
            Map(x => x.Published)
                .CustomType<CustomType>()
                .Not.Nullable();
            Map(x => x.Fiction)
                .CustomType<CustomSimpleType>()
                .Not.Nullable();
            References(x => x.Author)
                .Column("author_id")
                .Not.Nullable();
        }
    }
}
