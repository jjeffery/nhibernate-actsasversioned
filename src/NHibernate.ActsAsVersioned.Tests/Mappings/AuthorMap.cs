using FluentNHibernate.Mapping;
using NHibernate.ActsAsVersioned.Models;
// ReSharper disable VirtualMemberCallInConstructor

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
            Component(x => x.HomeAddress).ColumnPrefix("home_");
            HasMany(x => x.Books);
        }
    }
}
