using System;

namespace NHibernate.ActsAsVersioned
{
    /// <summary>
    /// Indicates that this property is updated automatically. This property is tracked in the
    /// versions table, but if an entity is updated and only non versioned and auto update columns
    /// are modified, then no new row will be created in the versions table. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AutoUpdateAttribute : Attribute
    {
    }
}