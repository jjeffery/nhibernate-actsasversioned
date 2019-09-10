using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NHibernate.ActsAsVersioned.Internal
{
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
    }
}
