using System;
using System.Collections.Generic;
using NHibernate.Event;
using NHibernate.Persister.Entity;
using NHibernate.Mapping;
using NHibernate.Type;

namespace NHibernate.ActsAsVersioned.Internal
{
    /// <summary>
    /// Keeps track of the work to be performed for an entity just prior
    /// to transaction completion.
    /// </summary>
    public abstract class WorkUnit
    {
        public readonly Tuple<string, object> Key;
        public readonly IEventSource Session;
        public readonly object Entity;
        public readonly object Id;
        public readonly IEntityPersister Persister;
        public readonly VersionedClass VersionedClass;

        protected WorkUnit(AbstractPostDatabaseOperationEvent @event, VersionedClass versionedClass)
        {
            Session = @event.Session;
            Entity = @event.Entity;
            Id = @event.Id;
            Persister = @event.Persister;
            VersionedClass = versionedClass;
            Key = new Tuple<string, object>(VersionedClass.VersionedEntityName, Id);
        }

        /// <summary>
        /// Merge this work unit with a subsequent work unit.
        /// </summary>
        /// <param name="second">The subsequent work unit.</param>
        /// <returns>A single work unit that combines the two.</returns>
        /// <remarks>
        /// This is necessary if a transaction has one or more insert/update/delete
        /// in a single transaction.
        /// </remarks>
        public WorkUnit Merge(WorkUnit second)
        {
            if (second is InsertWorkUnit ins)
            {
                return MergeInsert(ins);
            }

            if (second is UpdateWorkUnit upd)
            {
                return MergeUpdate(upd);
            }

            if (second is DeleteWorkUnit del)
            {
                return MergeDelete(del);
            }

            throw new ArgumentException("unknown work unit type");
        }

        /// <summary>
        /// Returns a dictionary of data to insert in the versions table, or null if no changes.
        /// </summary>
        public virtual IDictionary<string, object> GetData()
        {
            // each of the subclasses overrides this and adds more name/value pairs
            return new Dictionary<string, object> { { VersionedClass.RefIdPropertyName, Id } };
        }

        protected abstract WorkUnit MergeInsert(InsertWorkUnit second);
        protected abstract WorkUnit MergeUpdate(UpdateWorkUnit second);
        protected abstract WorkUnit MergeDelete(DeleteWorkUnit second);

        protected object GetValue(Property property, object[] state)
        {
            var index = System.Array.IndexOf(Persister.PropertyNames, property.Name);
            if (index < 0)
            {
                return null;
            }

            var value = state[index];

            // For entities, return the entity id.
            if (value != null && property.Type is EntityType entityType)
            {
                var entityName = entityType.GetAssociatedEntityName();
                var persister = Session.GetEntityPersister(entityName, value);
                var id = persister.GetIdentifier(value);
                value = id;
            }

            return value;
        }
    }

    public class InsertWorkUnit : WorkUnit
    {
        public object[] State { get; private set; }

        public InsertWorkUnit(PostInsertEvent @event, VersionedClass versionedClass) : base(@event, versionedClass)
        {
            State = Tools.CloneArray(@event.State);
        }

        public override IDictionary<string, object> GetData()
        {
            var data = base.GetData();

            foreach (var property in VersionedClass.Properties)
            {
                data.Add(property.Name, GetValue(property, State));
            }

            return data;
        }

        protected override WorkUnit MergeInsert(InsertWorkUnit second)
        {
            return second;
        }

        protected override WorkUnit MergeUpdate(UpdateWorkUnit second)
        {
            var clone = (InsertWorkUnit) MemberwiseClone();
            clone.State = Tools.CloneArray(second.State);
            return clone;
        }

        protected override WorkUnit MergeDelete(DeleteWorkUnit second)
        {
            return second;
        }
    }

    public class UpdateWorkUnit : WorkUnit
    {
        public object[] OldState { get; private set; }
        public object[] State { get; private set; }

        public UpdateWorkUnit(PostUpdateEvent @event, VersionedClass versionedClass) : base(@event, versionedClass)
        {
            OldState = Tools.CloneArray(@event.OldState);
            State = Tools.CloneArray(@event.State);
        }

        public override IDictionary<string, object> GetData()
        {
            var data = base.GetData();
            var different = false;

            foreach (var property in VersionedClass.Properties)
            {
                var newValue = GetValue(property, State);
                var oldValue = GetValue(property, OldState);

                if (!Tools.AreObjectsEqual(oldValue, newValue))
                {
                    // at least one property is different
                    different = true;
                }

                data.Add(property.Name, newValue);
            }

            if (!different)
            {
                // the old state and the new state are not different, at least 
                // if they are different it is in properties that are not versioned
                data = null;
            }

            return data;
        }

        protected override WorkUnit MergeInsert(InsertWorkUnit second)
        {
            // should not happen
            return second;
        }

        protected override WorkUnit MergeUpdate(UpdateWorkUnit second)
        {
            var clone = (UpdateWorkUnit) MemberwiseClone();
            clone.State = Tools.CloneArray(second.State);
            return clone;
        }

        protected override WorkUnit MergeDelete(DeleteWorkUnit second)
        {
            return second;
        }

        public UpdateWorkUnit WithOldState(object[] oldState)
        {
            var clone = (UpdateWorkUnit) MemberwiseClone();
            clone.OldState = Tools.CloneArray(oldState);
            return clone;
        }
    }

    public class DeleteWorkUnit : WorkUnit
    {
        public object[] DeletedState { get; }

        public DeleteWorkUnit(PostDeleteEvent @event, VersionedClass versionedClass) : base(@event, versionedClass)
        {
            DeletedState = Tools.CloneArray(@event.DeletedState);
        }

        public override IDictionary<string, object> GetData()
        {
            var data = base.GetData();

            foreach (var property in VersionedClass.Properties)
            {
                data.Add(property.Name, null);
            }

            return data;
        }

        protected override WorkUnit MergeInsert(InsertWorkUnit second)
        {
            // should not happen
            return second;
        }

        protected override WorkUnit MergeUpdate(UpdateWorkUnit second)
        {
            return second.WithOldState(DeletedState);
        }

        protected override WorkUnit MergeDelete(DeleteWorkUnit second)
        {
            return second;
        }
    }
}
