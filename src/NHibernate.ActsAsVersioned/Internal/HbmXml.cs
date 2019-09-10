using System.Xml.Linq;

namespace NHibernate.ActsAsVersioned.Internal
{
    public static class HbmXml
    {
        public static readonly XNamespace Namespace = "urn:nhibernate-mapping-2.2";

        public static XName ElementName(string name)
        {
            return Namespace + name;
        }

        public static XElement CreateProperty(this XElement parentElement, string name)
        {
            var propertyElem = new XElement(ElementName("property"),
                new XAttribute("name", name));
            parentElement.Add(propertyElem);
            return propertyElem;
        }

        public static XElement CreateComponent(this XElement parentElement, string name)
        {
            var propertyElem = new XElement(ElementName("component"),
                new XAttribute("name", name));
            parentElement.Add(propertyElem);
            return propertyElem;
        }
    }
}
