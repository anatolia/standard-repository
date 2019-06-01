using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

using StandardRepository.Factories;
using StandardRepository.Models.Entities;

namespace StandardRepository.Helpers.SqlExecutor
{
    public class SQLExecutor<TConnection, TCommand, TParameter> : SQLExecutorBase<TCommand, TParameter>, ISQLExecutor<TCommand, TParameter>
    where TConnection : DbConnection, new()
    where TCommand : DbCommand, new()
    where TParameter : DbParameter, new()
    {
        private readonly IConnectionFactory<TConnection> _connectionFactory;

        public SQLExecutor(IConnectionFactory<TConnection> connectionFactory, EntityUtils entityUtils) : base(entityUtils)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task ExecuteSql(string sql)
        {
            using (var connection = _connectionFactory.Create())
            {
                using (var commend = new TCommand())
                {
                    commend.Connection = connection;
                    await ExecuteSql(commend, sql);
                }
            }
        }

        public async Task<T> ExecuteStoredProcedureReturningValue<T>(string storedProcedureName, IEnumerable<TParameter> parameters = null)
        {
            using (var connection = _connectionFactory.Create())
            {
                using (var commend = new TCommand())
                {
                    commend.Connection = connection;
                    return await ExecuteStoredProcedureReturningValue<T>(commend, storedProcedureName, parameters);
                }
            }
        }

        public async Task ExecuteStoredProcedure(string storedProcedureName, List<TParameter> parameters = null)
        {
            using (var connection = _connectionFactory.Create())
            {
                using (var commend = new TCommand())
                {
                    commend.Connection = connection;
                    await ExecuteStoredProcedure(commend, storedProcedureName, parameters);
                }
            }
        }

        public async Task<T> ExecuteStoredProcedureReturningEntity<T>(string storedProcedureName, List<TParameter> parameters = null) where T : BaseEntity, new()
        {
            using (var connection = _connectionFactory.Create())
            {
                using (var commend = new TCommand())
                {
                    commend.Connection = connection;
                    return await ExecuteStoredProcedureReturningEntity<T>(commend, storedProcedureName, parameters);
                }
            }
        }

        public async Task ExecuteSql(string sql, List<TParameter> parameters = null)
        {
            using (var connection = _connectionFactory.Create())
            {
                using (var commend = new TCommand())
                {
                    commend.Connection = connection;
                    await ExecuteSql(commend, sql, parameters);
                }
            }
        }

        public async Task<T> ExecuteSqlReturningValue<T>(string sql, List<TParameter> parameters = null)
        {
            using (var connection = _connectionFactory.Create())
            {
                using (var commend = new TCommand())
                {
                    commend.Connection = connection;
                    return await ExecuteSqlReturningValue<T>(commend, sql, parameters);
                }
            }
        }

        public async Task<T> ExecuteSqlReturningEntity<T>(string sql, List<TParameter> parameters = null) where T : BaseEntity, new()
        {
            using (var connection = _connectionFactory.Create())
            {
                using (var commend = new TCommand())
                {
                    commend.Connection = connection;
                    return await ExecuteSqlReturningEntity<T>(commend, sql, parameters);
                }
            }
        }

        public async Task<List<T>> ExecuteSqlReturningEntityList<T>(string sql, List<TParameter> parameters = null) where T : BaseEntity, new()
        {
            using (var connection = _connectionFactory.Create())
            {
                using (var command = new TCommand())
                {
                    command.Connection = connection;
                    return await ExecuteSqlReturningEntityList<T>(command, sql, parameters);
                }
            }
        }

        public async Task<List<T>> ExecuteSqlReturningList<T>(string sql, List<TParameter> parameters = null)
        {
            using (var connection = _connectionFactory.Create())
            {
                using (var command = new TCommand())
                {
                    command.Connection = connection;
                    return await ExecuteSqlReturningList<T>(command, sql, parameters);
                }
            }
        }

        public async Task<List<EntityRevision<T>>> ExecuteSqlReturningRevisionList<T>(string sql, List<TParameter> parameters = null) where T : BaseEntity, new()
        {
            using (var connection = _connectionFactory.Create())
            {
                using (var commend = new TCommand())
                {
                    commend.Connection = connection;
                    return await ExecuteSqlReturningRevisionList<T>(commend, sql, parameters);
                }
            }
        }

        public List<string> ExecuteSqlForList(string sql)
        {
            using (var connection = _connectionFactory.Create())
            {
                using (var commend = new TCommand())
                {
                    commend.Connection = connection;
                    return base.ExecuteSqlForList(commend, sql);
                }
            }
        }
    }
}