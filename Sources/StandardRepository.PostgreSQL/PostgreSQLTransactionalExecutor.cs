using Npgsql;

using StandardRepository.Factories;

namespace StandardRepository.PostgreSQL
{
    public class PostgreSQLTransactionalExecutor : TransactionalExecutor<NpgsqlConnection, NpgsqlTransaction>
    {
        public PostgreSQLTransactionalExecutor(IConnectionFactory<NpgsqlConnection> connectionFactory) : base(connectionFactory)
        {
        }
    }
}