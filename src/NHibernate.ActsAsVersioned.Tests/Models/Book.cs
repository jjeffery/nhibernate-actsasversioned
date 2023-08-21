namespace NHibernate.ActsAsVersioned.Models
{
    [ActsAsVersioned("book_versions")]
    public class Book : Entity<int>
    {
        public virtual Author Author { get; set; }
        public virtual string Title { get; set; }
        public virtual bool Published { get; set; }
        public virtual bool Fiction { get; set; }
        [NotVersioned] public virtual int NotVersioned { get; set; }
        public virtual int LockVersion { get; set; }
        [AutoUpdate] public virtual int AutoUpdate { get; set; }
    }
}