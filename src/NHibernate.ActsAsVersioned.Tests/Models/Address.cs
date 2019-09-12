namespace NHibernate.ActsAsVersioned.Models
{
    public class Address
    {
        public virtual string Street { get; set; }
        public virtual string Locality { get; set; }
        public virtual string State { get; set; }
        public virtual string Postcode { get; set; }
    }
}
