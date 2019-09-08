using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using NHibernate.ActsAsVersioned.Attributes;
using NHibernate.ActsAsVersioned.Configuration;
using NHibernate.Mapping;
using NHibernate.Type;

[assembly:InternalsVisibleTo("NHibernate.ActsAsVersioned.Tests")]

// ReSharper disable once CheckNamespace
namespace NHibernate.Cfg
{
    /// <summary>
    /// Extension methods for NHibernate <see cref="Configuration"/> class.
    /// </summary>
    public static class NHConfigurationExtensions
    {
        /// <summary>
        /// Integrate ActsAsVersioned with NHibernate.
        /// </summary>
        /// <param name="configuration">The NHibernate configuration.</param>
        /// <returns>The NHibernate configuration.</returns>
        /// <remarks>
        /// WARNING: Be sure to call this method after set all configuration properties, after have added all your mappings 
        /// and after integrate NHibernate with all others packages as NHibernate.Validator, NHibernate.Search, NHibernate.Spatial.
        /// </remarks>
        public static Configuration IntegrateWithActsAsVersioned(this Configuration configuration)
        {
            var mappings = configuration.Mappings();
            foreach (var mapping in mappings)
            {
                configuration.AddXml(mapping.ToString());
            }

            return configuration;
        }

        public static IList<XDocument> Mappings(this Cfg.Configuration configuration)
        {

            var documents = new List<XDocument>();
            var mapping = configuration.BuildMapping();

            var versionedClasses = configuration.ClassMappings.Where(pc =>
                Attribute.GetCustomAttribute(pc.MappedClass, typeof(ActsAsVersionedAttribute)) != null).ToList();

            foreach (var pc in versionedClasses)
            {
                if (pc.IsAbstract ?? false)
                {
                    throw new InvalidOperationException($"Abstract versioned class not supported: {pc.EntityName}");
                }

                if (pc.Discriminator != null)
                {
                    throw new InvalidOperationException($"Inherited version class not supported: {pc.EntityName}");
                }

                if (pc.HasSubclasses)
                {
                    throw new InvalidOperationException($"Base versioned class not supported: {pc.EntityName}");
                }

                if (!pc.HasIdentifierProperty)
                {
                    throw new InvalidOperationException($"Class must have an identifer property: {pc.EntityName}");
                }

                var attribute =
                    Attribute.GetCustomAttribute(pc.MappedClass, typeof(ActsAsVersionedAttribute)) as
                        ActsAsVersionedAttribute;
                if (attribute == null)
                {
                    // should not happen
                    throw new InvalidOperationException("missing attribute");
                }

                var tableName = attribute.TableName;

                var mappingDocument = new XDocument();
                var classElem = mappingDocument.CreateEntity(tableName);

                var refIdPropertyName = pc.MappedClass.Name + pc.IdentifierProperty.Name;
                var refIdColumnName = Inflector.ToSnakeCase(refIdPropertyName);

                var refIdProperty = classElem.CreateProperty(refIdPropertyName);
                refIdProperty.Add(
                    new XAttribute("column", refIdColumnName),
                    new XAttribute("not-null", true),
                    new XAttribute("type", pc.IdentifierProperty.Type.Name));

                foreach (var property in pc.PropertyIterator)
                {
                    if (property.Type.IsCollectionType)
                    {
                        // not interested in collection types
                        continue;
                    }
                    var propertyName = property.Name;
                    var propertyElem = classElem.CreateProperty(propertyName);
                    if (property.ColumnIterator.FirstOrDefault(c => c is Column) is Column column)
                    {
                        propertyElem.Add(new XAttribute("column", column.Name));
                    }

                    if (property.IsEntityRelation)
                    {
                        var type = property.Type as EntityType;
                        var sqlType = type.SqlTypes(mapping).FirstOrDefault();
                        if (sqlType != null)
                        {
                            propertyElem.Add(new XAttribute("type", sqlType));
                        }
                        Debug.Assert(type != null);
                    }
                    else
                    {
                        propertyElem.Add(new XAttribute("type", property.Type.Name));
                    }

                    propertyElem.Add(new XAttribute("not-null", false));
                }

                documents.Add(mappingDocument);


            }

            return documents;
        }
    }
}
