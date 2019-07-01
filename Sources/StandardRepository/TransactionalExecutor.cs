using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace StandardRepository
{
    public class TransactionalExecutor<TConnection, TTransaction>
    where TConnection : DbConnection, new()
    where TTransaction : DbTransaction
    {
        private readonly TConnection _connection;

        public TransactionalExecutor(TConnection connection)
        {
            _connection = connection;
        }

        public async Task<TResult> ExecuteAsync<TResult>(Func<TConnection, Task<TResult>> func)
        {
            if (_connection.State == ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }

            using (var transaction = (TTransaction)_connection.BeginTransaction())
            {
                var result = await func(_connection);
                transaction.Commit();

                return result;
            }
        }
    }
}