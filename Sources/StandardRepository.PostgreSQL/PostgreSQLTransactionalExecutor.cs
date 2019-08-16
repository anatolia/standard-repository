using Npgsql;

namespace StandardRepository.PostgreSQL
{
    public class PostgreSQLTransactionalExecutor : TransactionalExecutor<NpgsqlConnection, NpgsqlTransaction>
    {
        public PostgreSQLTransactionalExecutor(NpgsqlConnection connection) : base(connection)
        {
        }
    }
}