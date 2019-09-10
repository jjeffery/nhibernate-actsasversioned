using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using NHibernate.ActsAsVersioned.Internal;
using NHibernate.Event;

[assembly:InternalsVisibleTo("NHibernate.ActsAsVersioned.Tests")]

// ReSharper disable once CheckNamespace
namespace NHibernate.Cfg
{
    /// <summary>
    /// Extension methods for NHibernate <see cref="Configuration"/> class for use with acts as versioned.
    /// </summary>
    public static class NHConfigurationExtensions
    {
        private static readonly INHibernateLogger Logger = NHibernateLogger.For(typeof(NHConfigurationExtensions));

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
            const string integratedKey = "ActsAsVersioned_Integrated";

            if (configuration.GetProperty(integratedKey) != null)
            {
                Logger.Warn($"{nameof(IntegrateWithActsAsVersioned)} has already been called for this configuration");
                return configuration;
            }

            var versionedClasses = new List<VersionedClass>();

            foreach (var pc in configuration.ClassMappings)
            {
                if (!VersionedClass.IsActsAsVersioned(pc))
                {
                    continue;
                }

                versionedClasses.Add(new VersionedClass(pc));
            }

            if (versionedClasses.Count == 0)
            {
                // nothing to do
                return configuration;
            }

            var mapping = configuration.BuildMapping();
            foreach (var vc in versionedClasses)
            {
                vc.UpdateConfiguration(configuration, mapping);
            }

            var listeners = new[] {new VersionedEventListener(versionedClasses)};
            // ReSharper disable CoVariantArrayConversion
            configuration.AppendListeners(ListenerType.PostInsert, listeners);
            configuration.AppendListeners(ListenerType.PostUpdate, listeners);
            configuration.AppendListeners(ListenerType.PostDelete, listeners);
            // ReSharper restore CoVariantArrayConversion

            configuration.SetProperty(integratedKey, "1");
            return configuration;
        }

        // Only used for testing. Will be removed.
        public static IList<XDocument> Mappings(this Cfg.Configuration configuration)
        {
            var versionedClasses = new List<VersionedClass>();

            foreach (var pc in configuration.ClassMappings)
            {
                if (!VersionedClass.IsActsAsVersioned(pc))
                {
                    continue;
                }

                versionedClasses.Add(new VersionedClass(pc));
            }

            var documents = new List<XDocument>();
            var mapping = configuration.BuildMapping();
            foreach (var vc in versionedClasses)
            {
                documents.Add(vc.BuildMappingDocument(configuration, mapping));
            }

            return documents;
        }
    }
}
