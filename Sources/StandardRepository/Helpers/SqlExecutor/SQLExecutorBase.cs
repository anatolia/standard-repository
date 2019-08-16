using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using StandardRepository.Models.Entities;

namespace StandardRepository.Helpers.SqlExecutor
{
    public class SQLExecutorBase<TCommand, TParameter>
    where TCommand : DbCommand, new()
    where TParameter : DbParameter, new()
    {
        private readonly EntityUtils _entityUtils;

        public SQLExecutorBase(EntityUtils entityUtils)
        {
            _entityUtils = entityUtils;
        }

        protected void AddParametersRange(TCommand command, IEnumerable<TParameter> parameters)
        {
            command.Parameters.AddRange((parameters ?? Enumerable.Empty<TParameter>()).ToArray());
        }

        public TParameter AddParameter<TValue>(TCommand command, string parameterName, TValue parameterValue,
                                               DbType? parameterType = null, int? size = null, byte? precision = null, byte? scale = null)
        {
            return AddParameter(command.Parameters, parameterName, parameterValue, parameterType, size, precision, scale);
        }

        public TParameter AddParameter<TValue>(DbParameterCollection parameters, string parameterName, TValue parameterValue,
                                               DbType? parameterType = null, int? size = null, byte? precision = null, byte? scale = null)
        {
            var parameter = CreateParameter(parameterName, parameterValue, parameterType, size, precision, scale);
            parameters.Add(parameter);
            return parameter;
        }

        public TParameter AddParameter<TValue>(ICollection<TParameter> parameters, string parameterName, TValue parameterValue,
                                               DbType? parameterType = null, int? size = null, byte? precision = null, byte? scale = null)
        {
            var parameter = CreateParameter(parameterName, parameterValue, parameterType, size, precision, scale);
            parameters.Add(parameter);
            return parameter;
        }

        public TParameter CreateParameter<TValue>(string parameterName, TValue parameterValue, DbType? parameterType = null,
                                                    int? size = null, byte? precision = null, byte? scale = null)
        {
            var parameter = new TParameter
            {
                ParameterName = parameterName,
                Value = parameterValue
            };

            if (parameterType.HasValue)
            {
                parameter.DbType = parameterType.Value;
            }

            if (size.HasValue)
            {
                parameter.Size = size.Value;
            }

            if (precision.HasValue)
            {
                parameter.Precision = precision.Value;
            }

            if (scale.HasValue)
            {
                parameter.Scale = scale.Value;
            }

            return parameter;
        }

        protected async Task ExecuteSql(TCommand command, string sql)
        {
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }

        protected async Task ExecuteSql(TCommand command, string sql, List<TParameter> parameters)
        {
            command.CommandText = sql;

            if (parameters != null)
            {
                for (var i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    command.Parameters.Add(parameter);
                }
            }

            await command.ExecuteNonQueryAsync();
        }

        protected async Task ExecuteStoredProcedure(TCommand command, string storedProcedureName, List<TParameter> parameters)
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedureName;

            AddParametersRange(command, parameters);

            await command.ExecuteNonQueryAsync();
        }

        protected async Task<T> ExecuteStoredProcedureReturningValue<T>(TCommand command, string storedProcedureName, IEnumerable<TParameter> parameters)
        {
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedureName;

            AddParametersRange(command, parameters);

            var result = await command.ExecuteScalarAsync();
            if (result == null)
            {
                throw new DataException($"{storedProcedureName} did not return value!");
            }

            var value = (T)result;
            return value;
        }
        
        protected async Task<T> ExecuteStoredProcedureReturningEntity<T>(TCommand command, string storedProcedureName, List<TParameter> parameters) where T : new()
        {
            var entity = new T();
            var properties = entity.GetType().GetProperties();
            var entityTypeName = entity.GetType().Name;

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedureName;

            AddParametersRange(command, parameters);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        _entityUtils.MapFields(reader, properties, entityTypeName, entity);
                    }
                }
            }

            return entity;
        }

        protected async Task<List<T>> ExecuteStoredProcedureReturningEntityList<T>(TCommand command, string storedProcedureName, List<TParameter> parameters) where T : new()
        {
            var items = new List<T>();
            
            var properties = typeof(T).GetProperties();
            var entityTypeName = typeof(T).Name;

            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = storedProcedureName;

            AddParametersRange(command, parameters);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var entity = new T();
                        _entityUtils.MapFields(reader, properties, entityTypeName, entity);
                        items.Add(entity);
                    }
                }
            }

            return items;
        }

        protected async Task<T> ExecuteSqlReturningValue<T>(TCommand command, string sql, List<TParameter> parameters)
        {
            command.CommandText = sql;
            command.Parameters.AddRange((parameters ?? Enumerable.Empty<TParameter>()).ToArray());

            var result = await command.ExecuteScalarAsync();
            if (result == null)
            {
                if (typeof(T) == typeof(bool))
                {
                    return (T)Convert.ChangeType(false, typeof(T));
                }

                throw new DataException("query did not return value!");
            }

            var value = (T)Convert.ChangeType(result, typeof(T));
            return value;
        }

        protected async Task<List<T>> ExecuteSqlReturningList<T>(TCommand command, string sql, List<TParameter> parameters)
        {
            var items = new List<T>();

            command.CommandText = sql;

            if (parameters != null)
            {
                for (var i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    command.Parameters.Add(parameter);
                }
            }

            command.Prepare();

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (!reader.HasRows)
                {
                    return items;
                }

                while (reader.Read())
                {
                    items.Add(reader.GetFieldValue<T>(0));
                }
            }

            return items;
        }

        protected async Task<T> ExecuteSqlReturningEntity<T>(TCommand command, string sql, List<TParameter> parameters) where T : new()
        {
            var entity = new T();
            var properties = entity.GetType().GetProperties();
            var entityTypeName = entity.GetType().Name;

            command.CommandText = sql;

            if (parameters != null)
            {
                for (var i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    command.Parameters.Add(parameter);
                }
            }

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        _entityUtils.MapFields(reader, properties, entityTypeName, entity);
                    }
                }
            }

            return entity;
        }

        protected async Task<List<T>> ExecuteSqlReturningEntityList<T>(TCommand command, string sql, List<TParameter> parameters) where T : BaseEntity, new()
        {
            var items = new List<T>();

            var properties = typeof(T).GetProperties();
            var entityTypeName = typeof(T).Name;

            command.CommandText = sql;

            if (parameters != null)
            {
                for (var i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    command.Parameters.Add(parameter);
                }
            }

            command.Prepare();

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var entity = new T();
                        _entityUtils.MapFields(reader, properties, entityTypeName, entity);
                        items.Add(entity);
                    }
                }
            }

            return items;
        }

        protected async Task<List<EntityRevision<T>>> ExecuteSqlReturningRevisionList<T>(TCommand command, string sql, List<TParameter> parameters) where T : BaseEntity, new()
        {
            var items = new List<EntityRevision<T>>();

            var properties = typeof(T).GetProperties();
            var entityTypeName = typeof(T).Name;

            command.CommandText = sql;

            if (parameters != null)
            {
                for (var i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];
                    command.Parameters.Add(parameter);
                }
            }

            command.Prepare();

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (!reader.HasRows)
                {
                    return items;
                }

                while (reader.Read())
                {
                    var revision = new EntityRevision<T>();
                    _entityUtils.MapFieldsRevision(reader, properties, entityTypeName, revision);
                    items.Add(revision);
                }
            }

            return items;
        }
    }
}