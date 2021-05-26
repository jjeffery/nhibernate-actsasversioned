using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Action;
using NHibernate.Event;

namespace NHibernate.ActsAsVersioned.Internal
{
    /// <summary>
    /// Manages all versioned transaction processors.
    /// </summary>
    /// <remarks>
    /// There is one versioned transaction processor for each transaction in progress that modifies
    /// a versioned entity.
    /// </remarks>
    public class VersionedTransactionManager
    {
        private ConcurrentDictionary<ITransaction, VersionedTransactionProcessor> _processors =
            new ConcurrentDictionary<ITransaction, VersionedTransactionProcessor>();

        /// <summary>
        /// Gets the versioned transaction processor associated with the transaction.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the session does not have a transaction.</exception>
        public VersionedTransactionProcessor Get(IEventSource session)
        {
            var transaction = session.GetCurrentTransaction();
            if (transaction == null)
            {
                throw new InvalidOperationException("acts as versioned: transaction is required");
            }

            return _processors.GetOrAdd(transaction, tx =>
            {
                var vp = new VersionedTransactionProcessor(session);

                // Register with the session so that the versioned transaction processor is invoked
                // just prior to the transaction commit, and that it is cleaned up after the transaction
                // has completed.
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
                if (_processors.TryGetValue(_transaction, out var processor))
                {
                    return processor.DoBeforeTransactionCompletionAsync(cancellationToken);
                }

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
