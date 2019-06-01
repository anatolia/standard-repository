using System;
using System.Data.Common;
using System.Threading.Tasks;

using StandardRepository.Factories;

namespace StandardRepository
{
    public class TransactionalExecutor<TConnection, TTransaction>
    where TConnection : DbConnection, new()
    where TTransaction : DbTransaction
    {
        private readonly IConnectionFactory<TConnection> _connectionFactory;

        public TransactionalExecutor(IConnectionFactory<TConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<IConnectionFactory<TConnection>, Task<TResult>> func)
        {
            using (var connection = _connectionFactory.Create())
            {
                using (var transaction = (TTransaction)connection.BeginTransaction())
                {
                    var result = await func(_connectionFactory);

                    transaction.Commit();
                    return result;
                }
            }
        }
    }
}