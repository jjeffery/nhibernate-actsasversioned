using FluentNHibernate.Mapping;
using NHibernate.ActsAsVersioned.Models;

namespace NHibernate.ActsAsVersioned.Mappings
{
    public class AddressMap : ComponentMap<Address>
    {
        public AddressMap()
        {
            Map(x => x.Street).Column("street");
            Map(x => x.Locality).Column("locality");
            Map(x => x.State).Column("state");
            Map(x => x.Postcode).Column("postcode");
        }
    }
}
