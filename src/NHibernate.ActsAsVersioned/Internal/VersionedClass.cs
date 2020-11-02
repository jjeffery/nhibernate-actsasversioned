using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using NHibernate.Engine;
using NHibernate.Mapping;
using NHibernate.Type;
using Configuration = NHibernate.Cfg.Configuration;

namespace NHibernate.ActsAsVersioned.Internal
{
    public class VersionedClass
    {
        // The persistent class information for the tracked class
        public readonly PersistentClass TrackedPersistentClass;

        // Table name for the versioned table
        public readonly string TableName;

        // Column name for the primary key of the versioned class
        public readonly string ColumnName;

        // NHibernate entity name for the versioned class
        public readonly string VersionedEntityName;

        // Name of the property in the versioned persistent class for referring to the tracked entity
        public readonly string RefIdPropertyName;

        // The persistent class information for the versioned class
        public PersistentClass VersionedPersistentClass { get; private set; }

        public readonly IList<Property> Properties = new List<Property>();

        public static bool IsActsAsVersioned(PersistentClass pc)
        {
            return pc.MappedClass != null &&
                   Attribute.GetCustomAttribute(pc.MappedClass, typeof(ActsAsVersionedAttribute)) != null;
        }

        public VersionedClass(PersistentClass pc)
        {
            if (pc.IsAbstract ?? false)
            {
                throw new NotSupportedException($"Abstract versioned class not supported: {pc.EntityName}");
            }

            if (pc.Discriminator != null)
            {
                throw new NotSupportedException($"Inherited version class not supported: {pc.EntityName}");
            }

            if (pc.HasSubclasses)
            {
                throw new NotSupportedException($"Base versioned class not supported: {pc.EntityName}");
            }

            if (!pc.HasIdentifierProperty)
            {
                throw new NotSupportedException($"Class must have an identifier property: {pc.EntityName}");
            }

            var attribute =
                Attribute.GetCustomAttribute(pc.MappedClass, typeof(ActsAsVersionedAttribute)) as
                    ActsAsVersionedAttribute;
            if (attribute == null)
            {
                // should not happen
                throw new ArgumentException($"Class is missing [ActsAsVersion] attribute: {pc.EntityName}");
            }

            TrackedPersistentClass = pc;
            TableName = attribute.TableName ?? Inflector.ToSnakeCase(pc.MappedClass.Name) + "_versions";
            ColumnName = attribute.ColumnName ?? Inflector.ToSnakeCase(pc.MappedClass.Name) + "_id";
            VersionedEntityName = pc.MappedClass + "_Version";
            RefIdPropertyName = pc.MappedClass.Name + pc.IdentifierProperty.Name;

            var notMapped = new HashSet<string>();
            foreach (var property in pc.MappedClass.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var notVersionedAttribute = Attribute.GetCustomAttribute(property, typeof(NotVersionedAttribute));
                if (notVersionedAttribute != null)
                {
                    notMapped.Add(property.Name);
                }
            }

            foreach (var property in pc.PropertyIterator)
            {
                if (property.Type.IsCollectionType)
                {
                    // not interested in collection types
                    continue;
                }

                if (notMapped.Contains(property.Name))
                {
                    continue;
                }

                Properties.Add(property);
            }
        }

        public void UpdateConfiguration(Configuration configuration, IMapping mapping = null)
        {
            var document = BuildMappingDocument(configuration, mapping);
            configuration.AddXml(document.ToString());

            VersionedPersistentClass = configuration.ClassMappings.First(c => c.EntityName == VersionedEntityName);
        }

        public XDocument BuildMappingDocument(Configuration configuration, IMapping mapping = null)
        {
            if (mapping == null)
            {
                mapping = configuration.BuildMapping();
            }

            var idElement = new XElement(HbmXml.ElementName("id"),
                new XAttribute("name", "Id"),
                new XAttribute("column", "id"),
                new XAttribute("type", typeof(Int32).Name),
                new XElement(HbmXml.ElementName("generator"),
                    new XAttribute("class", "native")));

            var classElement = new XElement(HbmXml.ElementName("class"),
                new XAttribute("entity-name", VersionedEntityName),
                new XAttribute("table", TableName),
                idElement);


            var refIdProperty = classElement.CreateProperty(RefIdPropertyName);
            refIdProperty.Add(
                new XAttribute("column", ColumnName),
                new XAttribute("not-null", true),
                new XAttribute("type", TrackedPersistentClass.IdentifierProperty.Type.Name));

            foreach (var property in TrackedPersistentClass.PropertyIterator)
            {
                if (Properties.Any(p => p.Name == property.Name))
                {
                    AddProperty(classElement, property, mapping);
                }
            }

            var rootElement = new XElement(HbmXml.ElementName("hibernate-mapping"),
                new XAttribute("auto-import", false),
                classElement);

            return new XDocument(rootElement);
        }

        private static void AddProperty(XElement parentElement, Property property, IMapping mapping)
        {
            if (property.Type.IsCollectionType)
            {
                // not interested in collection types
                return;
            }

            if (property.Value is Component component)
            {
                var componentElement = parentElement.CreateComponent(property.Name);
                var componentClass = component.ComponentClass?.AssemblyQualifiedName;
                if (componentClass != null)
                {
                    componentElement.Add(new XAttribute("class", componentClass));
                }

                foreach (var p in component.PropertyIterator)
                {
                    AddProperty(componentElement, p, mapping);
                }

                return;
            }

            var propertyElement = parentElement.CreateProperty(property.Name);
            if (property.ColumnIterator.FirstOrDefault(c => c is Column) is Column column)
            {
                propertyElement.Add(new XAttribute("column", column.Name));
            }

            if (property.Type is EntityType entityType)
            {
                var idType = entityType.GetIdentifierOrUniqueKeyType(mapping);
                AddTypeAttribute(propertyElement, idType);
            }
            else
            {
                AddTypeAttribute(propertyElement, property.Type);
            }

            propertyElement.Add(new XAttribute("not-null", false));
        }

        private static void AddTypeAttribute(XElement propertyElement, IType type)
        {
            if (type is CustomType customType)
            {
                propertyElement.Add(new XAttribute("type", customType.UserType.GetType().AssemblyQualifiedName));
            }
            else if (NHibernate.Type.TypeFactory.HeuristicType(type.Name) == null)
            {
                propertyElement.Add(new XAttribute("type", type.GetType().AssemblyQualifiedName));
            }
            else
            {
                propertyElement.Add(new XAttribute("type", type.Name));
            }
        }
    }
}
