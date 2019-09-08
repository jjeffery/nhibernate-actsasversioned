using NHibernate.ActsAsVersioned.Models;
using NHibernate.Cfg;
using NHibernate.Dialect;
using Xunit;
using Xunit.Abstractions;

namespace NHibernate.ActsAsVersioned
{
    public class TestConfigurationTests
    {
        public readonly ITestOutputHelper Output;
        public readonly ISessionFactory SessionFactory;

        public TestConfigurationTests(ITestOutputHelper output)
        {
            Output = output;
            SessionFactory = TestConfiguration.SessionFactory;
        }

        [Fact]
        public void Can_perform_queries()
        {
            using (var session = SessionFactory.OpenSession())
            {
                session.CreateSchema();
                using (var tx = session.BeginTransaction())
                {
                    var a1 = new Author { Name = "Author 1" };
                    var a2 = new Author { Name = "Author 2" };
                    session.Save(a1);
                    session.Save(a2);

                    var list1 = session.QueryOver<Author>().List();
                    Assert.Equal(2, list1.Count);

                    var list2 = session.QueryOver<Author>()
                        .WhereRestrictionOn(a => a.Name).IsLike("%2")
                        .List();

                    Assert.Equal(1, list2.Count);

                    tx.Rollback();
                }
            }
        }

        [Fact]
        public void Schema()
        {
            var cfg = TestConfiguration.Configuration;
            var dialect = new SQLiteDialect();
            var lines = cfg.GenerateSchemaCreationScript(dialect);
            foreach (var line in lines)
            {
                Output.WriteLine(line);
            }
        }

        [Fact]
        public void IntegratedSchema()
        {
            var cfg = TestConfiguration.Configuration.IntegrateWithActsAsVersioned();
            var dialect = new SQLiteDialect();
            var lines = cfg.GenerateSchemaCreationScript(dialect);
            foreach (var line in lines)
            {
                Output.WriteLine(line);
            }
        }

    }
}
