using System;

namespace NHibernate.ActsAsVersioned
{
    /// <summary>
    /// Attribute that indicates that an entity class has its version
    /// history recorded.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ActsAsVersionedAttribute : Attribute
    {
        /// <summary>
        /// Table name for the versioned table.
        /// </summary>
        public readonly string TableName;

        public ActsAsVersionedAttribute(string tableName)
        {
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }
    }
}
