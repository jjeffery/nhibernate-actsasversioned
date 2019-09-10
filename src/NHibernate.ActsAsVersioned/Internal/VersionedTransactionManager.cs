using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Action;
using NHibernate.Event;

namespace NHibernate.ActsAsVersioned.Internal
{
    public class VersionedTransactionManager
    {
        private ConcurrentDictionary<ITransaction, VersionedTransactionProcessor> _processors =
            new ConcurrentDictionary<ITransaction, VersionedTransactionProcessor>();

        public VersionedTransactionProcessor Get(IEventSource session)
        {
            var transaction = session.Transaction;
            if (transaction == null)
            {
                throw new InvalidOperationException("acts as versioned: transaction is required");
            }

            return _processors.GetOrAdd(transaction, tx =>
            {
                var vp = new VersionedTransactionProcessor(session);
                var transactionCompletionProcess = new TransactionCompletionProcess(_processors, tx);
                session.ActionQueue.RegisterProcess((IBeforeTransactionCompletionProcess) transactionCompletionProcess);
                session.ActionQueue.RegisterProcess((IAfterTransactionCompletionProcess)transactionCompletionProcess);
                return vp;
            });
        }

        private class TransactionCompletionProcess : IBeforeTransactionCompletionProcess, IAfterTransactionCompletionProcess
        {
            private readonly IDictionary<ITransaction, VersionedTransactionProcessor> _processors;
            private readonly ITransaction _transaction;

            public TransactionCompletionProcess(IDictionary<ITransaction, VersionedTransactionProcessor> processors,
                ITransaction transaction)
            {
                _processors = processors;
                _transaction = transaction;
            }

            public void ExecuteBeforeTransactionCompletion()
            {
                if (_processors.TryGetValue(_transaction, out var processor))
                {
                    processor.DoBeforeTransactionCompletion();
                }
            }

            public Task ExecuteBeforeTransactionCompletionAsync(CancellationToken cancellationToken)
            {
                ExecuteBeforeTransactionCompletion();
                return Task.CompletedTask;
            }

            public void ExecuteAfterTransactionCompletion(bool success)
            {
                _processors.Remove(_transaction);
            }

            public Task ExecuteAfterTransactionCompletionAsync(bool success, CancellationToken cancellationToken)
            {
                ExecuteAfterTransactionCompletion(success);
                return Task.CompletedTask;
            }
        }
    }
}
