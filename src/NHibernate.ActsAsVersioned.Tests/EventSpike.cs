using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.ActsAsVersioned.Models;
using NHibernate.Event;
using Xunit;
using Xunit.Abstractions;

namespace NHibernate.ActsAsVersioned
{
    public class EventSpike
    {
        public readonly ITestOutputHelper Output;
        public EventSpike(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void Test1()
        {
            var cfg = TestConfiguration.BuildNHibernateConfiguration();
            var listeners = new [] { new TestEventListener(Output)};
            cfg.AppendListeners(ListenerType.PostInsert, listeners);
            var sf = cfg.BuildSessionFactory();

            using (var session = sf.OpenSession())
            {
                session.CreateSchema();
                using (var tx = session.BeginTransaction())
                {
                    var a1 = new Author {Name = "Author 1"};
                    session.Save(a1);
                    tx.Commit();
                }
            }
        }
    }

    public class TestEventListener : IPostInsertEventListener
    {
        public readonly ITestOutputHelper Output;

        public TestEventListener(ITestOutputHelper output)
        {
            Output = output;
        }

        public Task OnPostInsertAsync(PostInsertEvent @event, CancellationToken cancellationToken)
        {
            OnPostInsert(@event);
            return Task.CompletedTask;
        }

        public void OnPostInsert(PostInsertEvent @event)
        {
            Output.WriteLine("PostInsertEvent", @event);
        }
    }
}
