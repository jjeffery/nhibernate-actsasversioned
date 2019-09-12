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

        private readonly ConcurrentDictionary<Tuple<string, object>, WorkUnit> _workUnits =
            new ConcurrentDictionary<Tuple<string, object>, WorkUnit>();

        public VersionedTransactionProcessor(ISession session)
        {
            _session = session;
        }

        public void Add(WorkUnit workUnit)
        {
            WorkUnit AddValue(Tuple<string, object> key) => workUnit;
            WorkUnit UpdateValue(Tuple<string, object> key, WorkUnit previousWorkUnit) => previousWorkUnit.Merge(workUnit);

            _workUnits.AddOrUpdate(workUnit.Key, AddValue, UpdateValue);
        }

        public void DoBeforeTransactionCompletion()
        {
            foreach (var kv in _workUnits)
            {
                var entityName = kv.Key.Item1;
                var workUnit = kv.Value;
                var data = workUnit.GetData();

                // if data is null then nothing has changed (or at least none of the
                // properties that are being versioned have changed)
                if (data != null)
                {
                    _session.Save(entityName, data);
                }
            }
        }

        public async Task DoBeforeTransactionCompletionAsync(CancellationToken cancellationToken)
        {
            foreach (var kv in _workUnits)
            {
                var entityName = kv.Key.Item1;
                var workUnit = kv.Value;
                var data = workUnit.GetData();
                if (data != null)
                {
                    await _session.SaveAsync(entityName, data, cancellationToken);
                }
            }
        }
    }
}
