using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

using StandardRepository.Factories;
using StandardRepository.Models.Entities;

namespace StandardRepository.Helpers.SqlExecutor
{
    public class SQLExecutor<TConnection, TCommand, TParameter> : SQLExecutorBase<TCommand, TParameter>, ISQLExecutor<TCommand, TConnection, TParameter>
    where TConnection : DbConnection, new()
    where TCommand : DbCommand, new()
    where TParameter : DbParameter, new()
    {
        private readonly IConnectionFactory<TConnection> _connectionFactory;
        private TConnection _connection;

        public SQLExecutor(IConnectionFactory<TConnection> connectionFactory, EntityUtils entityUtils) : base(entityUtils)
        {
            _connectionFactory = connectionFactory;
        }

        public void SetConnection(TConnection connection)
        {
            _connection = connection;
        }

        public async Task ExecuteSql(string sql)
        {
            using (var connection = _connectionFactory.Create())
            using (var command = new TCommand())
            {
                command.Connection = connection;
                await ExecuteSql(command, sql);
            }
        }

        public async Task<T> ExecuteStoredProcedureReturningValue<T>(string storedProcedureName, IEnumerable<TParameter> parameters = null)
        {
            if (_connection == null)
            {
                using (var command = new TCommand())
                {
                    command.Connection = _connection;
                    return await ExecuteStoredProcedureReturningValue<T>(command, storedProcedureName, parameters);
                }
            }

            using (var connection = _connectionFactory.Create())
            using (var command = new TCommand())
            {
                command.Connection = connection;
                return await ExecuteStoredProcedureReturningValue<T>(command, storedProcedureName, parameters);
            }
        }

        public async Task ExecuteStoredProcedure(string storedProcedureName, List<TParameter> parameters = null)
        {
            if (_connection == null)
            {
                using (var command = new TCommand())
                {
                    command.Connection = _connection;
                    await ExecuteStoredProcedure(command, storedProcedureName, parameters);
                }
            }
            else
            {
                using (var connection = _connectionFactory.Create())
                using (var command = new TCommand())
                {
                    command.Connection = connection;
                    await ExecuteStoredProcedure(command, storedProcedureName, parameters);
                }
            }
        }

        public async Task<T> ExecuteStoredProcedureReturningEntity<T>(string storedProcedureName, List<TParameter> parameters = null) where T : BaseEntity, new()
        {
            if (_connection != null)
            {
                using (var command = new TCommand())
                {
                    command.Connection = _connection;
                    return await ExecuteStoredProcedureReturningEntity<T>(command, storedProcedureName, parameters);
                }
            }

            using (var connection = _connectionFactory.Create())
            using (var command = new TCommand())
            {
                command.Connection = connection;
                return await ExecuteStoredProcedureReturningEntity<T>(command, storedProcedureName, parameters);
            }
        }

        public async Task ExecuteSql(string sql, List<TParameter> parameters)
        {
            if (_connection != null)
            {
                using (var command = new TCommand())
                {
                    command.Connection = _connection;
                    await ExecuteSql(command, sql, parameters);
                }
            }
            else
            {
                using (var connection = _connectionFactory.Create())
                using (var command = new TCommand())
                {
                    command.Connection = connection;
                    await ExecuteSql(command, sql, parameters);
                }
            }
        }

        public async Task<T> ExecuteSqlReturningValue<T>(string sql, List<TParameter> parameters = null)
        {
            if (_connection != null)
            {
                using (var command = new TCommand())
                {
                    command.Connection = _connection;
                    return await ExecuteSqlReturningValue<T>(command, sql, parameters);
                }
            }

            using (var connection = _connectionFactory.Create())
            using (var commend = new TCommand())
            {
                commend.Connection = connection;
                return await ExecuteSqlReturningValue<T>(commend, sql, parameters);
            }
        }

        public async Task<T> ExecuteSqlReturningEntity<T>(string sql, List<TParameter> parameters = null) where T : BaseEntity, new()
        {
            if (_connection != null)
            {
                using (var command = new TCommand())
                {
                    command.Connection = _connection;
                    return await ExecuteSqlReturningEntity<T>(command, sql, parameters);
                }
            }

            using (var connection = _connectionFactory.Create())
            using (var command = new TCommand())
            {
                command.Connection = connection;
                return await ExecuteSqlReturningEntity<T>(command, sql, parameters);
            }
        }

        public async Task<List<T>> ExecuteSqlReturningEntityList<T>(string sql, List<TParameter> parameters = null) where T : BaseEntity, new()
        {
            if (_connection != null)
            {
                using (var command = new TCommand())
                {
                    command.Connection = _connection;
                    return await ExecuteSqlReturningEntityList<T>(command, sql, parameters);
                }
            }

            using (var connection = _connectionFactory.Create())
            using (var command = new TCommand())
            {
                command.Connection = connection;
                return await ExecuteSqlReturningEntityList<T>(command, sql, parameters);
            }
        }

        public async Task<List<T>> ExecuteSqlReturningList<T>(string sql, List<TParameter> parameters = null)
        {
            if (_connection != null)
            {
                using (var command = new TCommand())
                {
                    command.Connection = _connection;
                    return await ExecuteSqlReturningList<T>(command, sql, parameters);
                }
            }

            using (var connection = _connectionFactory.Create())
            using (var command = new TCommand())
            {
                command.Connection = connection;
                return await ExecuteSqlReturningList<T>(command, sql, parameters);
            }
        }

        public async Task<List<EntityRevision<T>>> ExecuteSqlReturningRevisionList<T>(string sql, List<TParameter> parameters = null) where T : BaseEntity, new()
        {
            if (_connection != null)
            {
                using (var commend = new TCommand())
                {
                    commend.Connection = _connection;
                    return await ExecuteSqlReturningRevisionList<T>(commend, sql, parameters);
                }
            }

            using (var connection = _connectionFactory.Create())
            using (var commend = new TCommand())
            {
                commend.Connection = connection;
                return await ExecuteSqlReturningRevisionList<T>(commend, sql, parameters);
            }
        }
    }
}