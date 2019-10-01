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

        /// <summary>
        /// Name of the reference column for the versioned entity
        /// primary key.
        /// </summary>
        public string ColumnName { get; set; }

        public ActsAsVersionedAttribute(string tableName = null)
        {
            TableName = tableName;
        }
    }
}
