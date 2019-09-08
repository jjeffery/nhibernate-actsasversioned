using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace NHibernate.ActsAsVersioned.Configuration
{
    public static class XmlUtils
    {
        private static readonly XNamespace xNamespace = "urn:nhibernate-mapping-2.2";
        private static readonly string AssemblyName = Assembly.GetExecutingAssembly().FullName;

        public static XName CreateElementName(string name)
        {
            return xNamespace + name;
        }

        public static XElement CreateEntity(this XDocument document, string tableName)
        {
            var idElem = new XElement(CreateElementName("id"),
                new XAttribute("name", "Id"),
                new XAttribute("column", "id"),
                new XAttribute("type", typeof(Int32).Name),
                new XElement(CreateElementName("generator"),
                    new XAttribute("class", "native")));

            var classElem = new XElement(CreateElementName("class"),
                new XAttribute("entity-name", tableName),
                new XAttribute("table", tableName),
                idElem);

            var mappingElem = new XElement(CreateElementName("hibernate-mapping"),
                new XAttribute("assembly", AssemblyName),
                new XAttribute("auto-import", false),
                classElem);

            document.Add(mappingElem);

            return classElem;
        }

        public static XElement CreateProperty(this XElement classElement, string propertyName)
        {
            var propertyElem = new XElement(CreateElementName("property"),
                new XAttribute("name", propertyName));
            classElement.Add(propertyElem);
            return propertyElem;
        }
    }
}
