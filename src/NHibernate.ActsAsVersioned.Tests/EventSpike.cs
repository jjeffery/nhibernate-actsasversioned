using System.Collections.Generic;
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
            TestConfiguration.DataSource = @"D:\temp\event_spike.db";
            var cfg = TestConfiguration.BuildNHibernateConfiguration();
            //var listeners = new [] { new TestEventListener(Output)};
            //cfg.AppendListeners(ListenerType.PostInsert, listeners);
            var sf = cfg.BuildSessionFactory();

            using (var session = sf.OpenSession())
            {
                session.CreateSchema();
                int a1Id, b1Id;
                using (var tx = session.BeginTransaction())
                {
                    var a1 = new Author
                    {
                        Name = "Author 1",
                        HomeAddress = new Address
                        {
                            Street = "1313 Elm St",
                            Locality = "Woodsville",
                            State = "VIC",
                            Postcode = "3181"
                        }
                    };
                    session.Save(a1);
                    a1Id = a1.Id;

                    var b1 = new Book
                    {
                        Author = a1,
                        Title = "NHibernate for dummies"
                    };
                    session.Save(b1);
                    b1Id = b1.Id;

                    a1.HomeAddress.Street = "123 Selwood Drive";
                    a1.HomeAddress.Locality = "New Tudsbury";
                    a1.HomeAddress.State = "WA";
                    a1.HomeAddress.Postcode = null;

                    session.Update(a1);

                    tx.Commit();
                }

                using (var tx = session.BeginTransaction())
                {
                    var a1 = session.Get<Author>(a1Id);
                    a1.HomeAddress = new Address()
                    {
                        Street = "1313 Elm St",
                        Locality = "Woodsville",
                        State = "VIC",
                        Postcode = "3181"
                    };
                    tx.Commit();
                }

                using (var tx = session.BeginTransaction())
                {
                    var b1 = session.Get<Book>(b1Id);
                    session.Delete(b1);

                    var a1 = session.Get<Author>(a1Id);
                    session.Delete(a1);
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
            if (@event.Entity is Book b)
            {
                var dict = new Dictionary<string, object>
                {
                    {"BookId", @event.Id},
                    {"Title", b.Title },
                    {"Author", b.Author?.Id }
                };

                @event.Session.Save("NHibernate.ActsAsVersioned.Models.Book_Version", dict);
            }
        }
    }
}
