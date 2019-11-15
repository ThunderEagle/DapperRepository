
using System.Transactions;

namespace DapperRepository
{
    public static class TransactionScopeHelper
    {
        public static TransactionScope CreateNew()
        {
            return new TransactionScope(TransactionScopeOption.Required,
                                        new TransactionOptions
                                        {
                                            IsolationLevel = IsolationLevel.ReadUncommitted
                                        }, TransactionScopeAsyncFlowOption.Enabled);

        }

        public static TransactionScope CreateNewReadCommitted()
        {
            return new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}