using System;

namespace NHibernate.ActsAsVersioned
{
    /// <summary>
    /// Indicates that changes to this property are not tracked in the versions table.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotVersionedAttribute : Attribute
    {
    }
}
