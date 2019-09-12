using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Event;

namespace NHibernate.ActsAsVersioned.Internal
{
    /// <summary>
    /// Listener for NHibernate events.
    /// </summary>
    public class VersionedEventListener : IPostInsertEventListener, IPostUpdateEventListener, IPostDeleteEventListener
    {
        // versioned classes, keyed by the entity name of the tracked class
        private readonly IDictionary<string, VersionedClass> _versionedClasses = new Dictionary<string, VersionedClass>();

        // maintains a transaction processor for each transaction in progress
        private readonly VersionedTransactionManager _transactionManager = new VersionedTransactionManager();

        public VersionedEventListener(IEnumerable<VersionedClass> versionedClasses)
        {
            foreach (var vc in versionedClasses)
            {
                _versionedClasses.Add(vc.TrackedPersistentClass.EntityName, vc);
            }
        }
        
        public Task OnPostInsertAsync(PostInsertEvent @event, CancellationToken cancellationToken)
        {
            OnPostInsert(@event);
            return Task.CompletedTask;
        }

        public void OnPostInsert(PostInsertEvent @event)
        {
            var vc = GetVersionedClass(@event);
            if (vc == null)
            {
                return;
            }
            var processor = _transactionManager.Get(@event.Session);
            processor.Add(new InsertWorkUnit(@event, vc));
        }

        public Task OnPostUpdateAsync(PostUpdateEvent @event, CancellationToken cancellationToken)
        {
            OnPostUpdate(@event);
            return Task.CompletedTask;
        }

        public void OnPostUpdate(PostUpdateEvent @event)
        {
            var vc = GetVersionedClass(@event);
            if (vc == null)
            {
                return;
            }
            var processor = _transactionManager.Get(@event.Session);
            processor.Add(new UpdateWorkUnit(@event, vc));
        }

        public Task OnPostDeleteAsync(PostDeleteEvent @event, CancellationToken cancellationToken)
        {
            OnPostDelete(@event);
            return Task.CompletedTask;
        }

        public void OnPostDelete(PostDeleteEvent @event)
        {
            var vc = GetVersionedClass(@event);
            if (vc == null)
            {
                return;
            }
            var processor = _transactionManager.Get(@event.Session);
            processor.Add(new DeleteWorkUnit(@event, vc));
        }

        private VersionedClass GetVersionedClass(AbstractPostDatabaseOperationEvent @event)
        {
            if (_versionedClasses.TryGetValue(@event.Persister.EntityName, out var vc))
            {
                return vc;
            }

            return null;
        }
    }
}
