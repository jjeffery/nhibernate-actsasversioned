using System.Collections.Generic;

namespace NHibernate.ActsAsVersioned.Models
{
    [ActsAsVersioned("author_versions")]
    public class Author : Entity<int>
    {
        public virtual string Name { get; set; }
        public virtual Address HomeAddress { get; set; }
        public virtual IList<Book> Books { get; set; }
    }
}
