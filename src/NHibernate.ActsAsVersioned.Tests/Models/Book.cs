using System;
using System.Collections.Generic;
using System.Text;
using NHibernate.ActsAsVersioned.Attributes;

namespace NHibernate.ActsAsVersioned.Models
{
    [ActsAsVersioned("book_versions")]
    public class Book : Entity<int>
    {
        public virtual Author Author { get; set; }
        public virtual string Title { get; set; }
    }
}
