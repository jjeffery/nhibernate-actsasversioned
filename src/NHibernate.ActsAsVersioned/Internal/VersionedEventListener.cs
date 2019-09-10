using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Event;
using NHibernate.Type;

namespace NHibernate.ActsAsVersioned.Internal
{
    public class VersionedEventListener : IPostInsertEventListener, IPostUpdateEventListener, IPostDeleteEventListener
    {
        // versioned classes, keyed by the entity name of the tracked class
        private readonly IDictionary<string, VersionedClass> _versionedClasses = new Dictionary<string, VersionedClass>();

        private readonly VersionedTransactionManager _transactions = new VersionedTransactionManager();

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
            CommonInsertUpdate(@event);
        }

        public Task OnPostUpdateAsync(PostUpdateEvent @event, CancellationToken cancellationToken)
        {
            OnPostUpdate(@event);
            return Task.CompletedTask;
        }

        public void OnPostUpdate(PostUpdateEvent @event)
        {
            CommonInsertUpdate(@event);
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

            var data = new Dictionary<string, object> { [vc.RefIdPropertyName] = @event.Id };
            foreach (var property in vc.Properties)
            {
                data[property.Name] = null;
            }

            var processor = _transactions.Get(@event.Session);
            processor.Add(vc.VersionedEntityName, @event.Id, data);
        }

        private VersionedClass GetVersionedClass(AbstractPostDatabaseOperationEvent @event)
        {
            if (_versionedClasses.TryGetValue(@event.Persister.EntityName, out var vc))
            {
                return vc;
            }

            return null;
        }

        public void CommonInsertUpdate(AbstractPostDatabaseOperationEvent @event)
        {
            var vc = GetVersionedClass(@event);
            if (vc == null)
            {
                return;
            }

            var data = new Dictionary<string, object> { [vc.RefIdPropertyName] = @event.Id };
            foreach (var property in vc.Properties)
            {
                var value = @event.Persister.GetPropertyValue(@event.Entity, property.Name);
                if (property.Type is EntityType entityType)
                {
                    var entityName = entityType.GetAssociatedEntityName();
                    var persister = @event.Session.GetEntityPersister(entityName, value);
                    var id = persister.GetIdentifier(value);
                    data[property.Name] = id;
                }
                else
                {
                    data[property.Name] = value;
                }
            }

            var processor = _transactions.Get(@event.Session);
            processor.Add(vc.VersionedEntityName, @event.Id, data);
        }
    }
}
