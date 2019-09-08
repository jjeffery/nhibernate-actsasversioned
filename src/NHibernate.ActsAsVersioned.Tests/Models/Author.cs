using System.Collections.Generic;
using NHibernate.ActsAsVersioned.Attributes;

namespace NHibernate.ActsAsVersioned.Models
{
    [ActsAsVersioned("author_versions")]
    public class Author : Entity<int>
    {
        public virtual string Name { get; set; }
        public virtual IList<Book> Books { get; set; }
    }
}
