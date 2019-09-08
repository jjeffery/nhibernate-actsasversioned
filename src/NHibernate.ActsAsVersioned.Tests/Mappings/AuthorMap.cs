using FluentNHibernate.Mapping;
using NHibernate.ActsAsVersioned.Models;

namespace NHibernate.ActsAsVersioned.Mappings
{
    public class AuthorMap : ClassMap<Author>
    {
        public AuthorMap()
        {
            Table("authors");
            Id(x => x.Id).Column("id");
            Map(x => x.Name)
                .Column("name")
                .Not.Nullable();
            HasMany(x => x.Books);
        }
    }
}
