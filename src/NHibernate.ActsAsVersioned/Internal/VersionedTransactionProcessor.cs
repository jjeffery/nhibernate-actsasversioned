using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NHibernate.ActsAsVersioned.Internal
{
    /// <summary>
    /// Responsible for inserting rows into the version tables at the
    /// end of a transaction.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Whenever an entity is modified and that entity is marked as acts as versioned,
    /// the data for the entity is stored in the versioned transaction processor that
    /// is associated with the transaction.
    /// </para>
    /// <para>
    /// Just before the transaction is committed, the versioned transaction processor
    /// inserts one row for each of the entities that were modified. This results in
    /// exactly one row being inserted for each entity modified, regardless of how many
    /// times the entity has been updated in the database during the transaction.
    /// </para>
    /// </remarks>
    public class VersionedTransactionProcessor
    {
        private readonly ISession _session;

        private readonly ConcurrentDictionary<Tuple<string, object>, IDictionary<string, object>> _versionedObjects =
            new ConcurrentDictionary<Tuple<string, object>, IDictionary<string, object>>();

        public VersionedTransactionProcessor(ISession session)
        {
            _session = session;
        }

        public void Add(string entityName, object id, IDictionary<string, object> data)
        {
            var key = new Tuple<string, object>(entityName, id);
            _versionedObjects.AddOrUpdate(key, data, (existingKey, existingData) => data);
        }

        public void DoBeforeTransactionCompletion()
        {
            foreach (var kv in _versionedObjects)
            {
                var entityName = kv.Key.Item1;
                var data = kv.Value;
                _session.Save(entityName, data);
            }
        }

        public async Task DoBeforeTransactionCompletionAsync(CancellationToken cancellationToken)
        {
            foreach (var kv in _versionedObjects)
            {
                var entityName = kv.Key.Item1;
                var data = kv.Value;
                await _session.SaveAsync(entityName, data, cancellationToken);
            }
        }
    }
}
