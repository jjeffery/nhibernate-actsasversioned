using System;
using System.Data.SQLite;
using System.IO;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace NHibernate.ActsAsVersioned
{
    public static class TestConfiguration
    {
        public static string DataSource = ":memory:";

        private static readonly Lazy<Cfg.Configuration> LazyConfiguration =
            new Lazy<Cfg.Configuration>(BuildNHibernateConfiguration);

        private static readonly Lazy<ISessionFactory> LazySessionFactory =
            new Lazy<ISessionFactory>(BuildSessionFactory);

        public static Cfg.Configuration Configuration => LazyConfiguration.Value;
        public static ISessionFactory SessionFactory => LazySessionFactory.Value;


        public static ISessionFactory BuildSessionFactory()
        {
            return Configuration.BuildSessionFactory();
        }

        public static void CreateSchema(this ISession session)
        {
            var schemaUpdate = new SchemaExport(Configuration);
            schemaUpdate.Execute(false, true, false, session.Connection, TextWriter.Null);
        }

        public static NHibernate.Cfg.Configuration BuildNHibernateConfiguration()
        {
            var fluentConfig = BuildFluentConfiguration();
            var nhConfig = fluentConfig.BuildConfiguration();
            return nhConfig.IntegrateWithActsAsVersioned();
        }

        public static string BuildConnectionString()
        {
            var cs = new SQLiteConnectionStringBuilder
            {
                DataSource = DataSource
            };

            if (DataSource != ":memory:")
            {
                File.Delete(DataSource);
            }

            return cs.ToString();
        }

        private static FluentConfiguration BuildFluentConfiguration()
        {
            var connectionString = BuildConnectionString();
            var dbConfig = Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.ConnectionString(connectionString))
                .Mappings(m => { m.FluentMappings.AddFromAssemblyOf<Models.Author>(); })
                .ExposeConfiguration(c =>
                {
                    c.SetProperty(NHibernate.Cfg.Environment.ShowSql, "true");
                    // Set global batch size. Choosing a power of two because of the way NHibernate
                    // chooses smaller batch slices based on this parameter.
                    // See https://stackoverflow.com/questions/1264261/nhibernate-alternates-batch-size
                    // Also https://forum.hibernate.org/viewtopic.php?p=2422139
                    c.SetProperty(NHibernate.Cfg.Environment.DefaultBatchFetchSize, "128");
                });

            return dbConfig;
        }
    }
}
