using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Npgsql;
using NpgsqlTypes;

using StandardRepository.Factories;
using StandardRepository.Helpers;
using StandardRepository.Models;
using StandardRepository.Models.Entities;
using StandardRepository.PostgreSQL.Helpers;
using StandardRepository.PostgreSQL.Helpers.SqlExecutor;

namespace StandardRepository.PostgreSQL
{
    public class PostgreSQLRepository<T> : StandardRepository<T, NpgsqlConnection, NpgsqlCommand, NpgsqlParameter> where T : BaseEntity, new()
    {
        public override void SetSqlExecutorForTransaction(IConnectionFactory<NpgsqlConnection> connectionFactory)
        {
            SQLExecutor = new PostgreSQLExecutor(connectionFactory, _entityUtils);
        }

        protected override void AppendWhere(Expression<Func<T, bool>> @where, List<NpgsqlParameter> parameters, StringBuilder sb, bool isIncludeDeleted)
        {
            if (where != null)
            {
                var prmDictionary = new Dictionary<string, DbParameterInfo>();
                var conditions = _expressionUtils.GetConditions(where.Body, prmDictionary);
                AddToParameters(parameters, prmDictionary);

                if (!isIncludeDeleted)
                {
                    sb.AppendLine($"{PostgreSQLConstants.WHERE_IS_NOT_DELETED_AND} {conditions}");
                }
                else
                {
                    sb.AppendLine($"{SQLConstants.WHERE} {conditions}");
                }
            }
            else
            {
                if (!isIncludeDeleted)
                {
                    sb.AppendLine($"{PostgreSQLConstants.WHERE_IS_NOT_DELETED}");
                }
            }
        }

        private static void AddToParameters(List<NpgsqlParameter> parameters, Dictionary<string, DbParameterInfo> prmDictionary)
        {
            foreach (var prm in prmDictionary)
            {
                var parameter = new NpgsqlParameter(prm.Value.Name, prm.Value.DbType);
                parameter.Value = prm.Value.Value;
                parameters.Add(parameter);
            }
        }

        public override async Task<List<T>> SelectMany(Expression<Func<T, bool>> @where, int skip = 0, int take = 100,
                                                       Expression<Func<T, object>> orderByColumn = null, bool isAscending = true, bool isIncludeDeleted = false)
        {
            var sb = new StringBuilder();
            sb.Append($"{SQLConstants.SELECT} *{Environment.NewLine}");
            sb.Append($"{SQLConstants.FROM} {_sqlConstants.TableFullName}{Environment.NewLine}");

            var orderColumn = _sqlConstants.IdFieldName;
            if (orderByColumn != null)
            {
                orderColumn = _expressionUtils.GetField(orderByColumn.Body);
            }

            var ascOrDesc = SQLConstants.DESC;
            if (isAscending)
            {
                ascOrDesc = SQLConstants.ASC;
            }

            var parameters = new List<NpgsqlParameter>();
            var prmSkip = new NpgsqlParameter<int>(SQLConstants.SKIP_PARAMETER_NAME, NpgsqlDbType.Integer);
            prmSkip.TypedValue = skip;
            parameters.Add(prmSkip);
            var prmTake = new NpgsqlParameter<int>(SQLConstants.TAKE_PARAMETER_NAME, NpgsqlDbType.Integer);
            prmTake.TypedValue = take;
            parameters.Add(prmTake);

            AppendWhere(where, parameters, sb, isIncludeDeleted);
            sb.Append($"{SQLConstants.ORDER_BY} {orderColumn} {ascOrDesc}{Environment.NewLine}");
            sb.Append($"{PostgreSQLConstants.LIMIT} {PostgreSQLConstants.PARAMETER_PRESIGN}{SQLConstants.TAKE_PARAMETER_NAME} {PostgreSQLConstants.OFFSET} {PostgreSQLConstants.PARAMETER_PRESIGN}{SQLConstants.SKIP_PARAMETER_NAME} {Environment.NewLine}");

            var items = await SQLExecutor.ExecuteSqlReturningEntityList<T>(sb.ToString(), parameters);
            return items;
        }

        public override async Task<List<T>> SelectAfter(Expression<Func<T, bool>> @where, long lastId, int take = 100,
                                                  Expression<Func<T, object>> orderByColumn = null, bool isAscending = true, bool isIncludeDeleted = false)
        {
            var sb = new StringBuilder();
            sb.Append($"{SQLConstants.SELECT} *{Environment.NewLine}");
            sb.Append($"{SQLConstants.FROM} {_sqlConstants.TableFullName}{Environment.NewLine}");

            var orderColumn = _sqlConstants.IdFieldName;
            if (orderByColumn != null)
            {
                orderColumn = _expressionUtils.GetField(orderByColumn.Body);
            }

            var ascOrDesc = SQLConstants.DESC;
            if (isAscending)
            {
                ascOrDesc = SQLConstants.ASC;
            }

            var parameters = new List<NpgsqlParameter>();
            var prmTake = new NpgsqlParameter<int>(SQLConstants.TAKE_PARAMETER_NAME, NpgsqlDbType.Integer);
            prmTake.TypedValue = take;
            parameters.Add(prmTake);

            AppendWhere(where, parameters, sb, isIncludeDeleted);

            sb.Append($" {SQLConstants.AND} {_sqlConstants.IdFieldName} > {lastId}{Environment.NewLine}");
            sb.Append($"{SQLConstants.ORDER_BY} {orderColumn} {ascOrDesc}{Environment.NewLine}");
            sb.Append($"{PostgreSQLConstants.LIMIT} {PostgreSQLConstants.PARAMETER_PRESIGN}{SQLConstants.TAKE_PARAMETER_NAME} {PostgreSQLConstants.OFFSET} 0{Environment.NewLine}");

            var items = await SQLExecutor.ExecuteSqlReturningEntityList<T>(sb.ToString(), parameters);
            return items;
        }

        public override async Task<List<T>> SelectAfter(Expression<Func<T, bool>> @where, Guid lastUid, int take = 100,
                                                  Expression<Func<T, object>> orderByColumn = null, bool isAscending = true, bool isIncludeDeleted = false)
        {
            var sb = new StringBuilder();
            sb.Append($"{SQLConstants.SELECT} *{Environment.NewLine}");
            sb.Append($"{SQLConstants.FROM} {_sqlConstants.TableFullName}{Environment.NewLine}");

            var orderColumn = _sqlConstants.IdFieldName;
            if (orderByColumn != null)
            {
                orderColumn = _expressionUtils.GetField(orderByColumn.Body);
            }

            var ascOrDesc = SQLConstants.DESC;
            if (isAscending)
            {
                ascOrDesc = SQLConstants.ASC;
            }

            var parameters = new List<NpgsqlParameter>();
            var prmTake = new NpgsqlParameter<int>(SQLConstants.TAKE_PARAMETER_NAME, NpgsqlDbType.Integer);
            prmTake.TypedValue = take;
            parameters.Add(prmTake);

            AppendWhere(where, parameters, sb, isIncludeDeleted);

            if (lastUid != Guid.Empty)
            {
                var prmLastUid = new NpgsqlParameter<Guid>(SQLConstants.LAST_UID_PARAMETER_NAME, NpgsqlDbType.Uuid);
                prmLastUid.TypedValue = lastUid;
                parameters.Add(prmLastUid);

                var schemaName = _entityUtils.GetSchemaName(typeof(T));
                var tableName = _entityUtils.GetTableName(typeof(T));

                if (where == null && !isIncludeDeleted)
                {
                    sb.Append($"{SQLConstants.WHERE}");
                }
                else
                {
                    sb.Append($" {SQLConstants.AND}");
                }

                sb.Append($" {_sqlConstants.IdFieldName} > ({SQLConstants.SELECT} {tableName}_id {SQLConstants.FROM} {schemaName}.{tableName} {SQLConstants.WHERE} {tableName}_uid = {PostgreSQLConstants.PARAMETER_PRESIGN}{SQLConstants.LAST_UID_PARAMETER_NAME}){Environment.NewLine}");
            }

            sb.Append($"{SQLConstants.ORDER_BY} {orderColumn} {ascOrDesc}{Environment.NewLine}");
            sb.Append($"{PostgreSQLConstants.LIMIT} {PostgreSQLConstants.PARAMETER_PRESIGN}{SQLConstants.TAKE_PARAMETER_NAME} {PostgreSQLConstants.OFFSET} 0{Environment.NewLine}");

            var items = await SQLExecutor.ExecuteSqlReturningEntityList<T>(sb.ToString(), parameters);
            return items;
        }

        public override async Task<List<long>> SelectIds(Expression<Func<T, bool>> @where, bool isIncludeDeleted = false)
        {
            var sb = new StringBuilder();
            sb.Append($"{SQLConstants.SELECT} {_sqlConstants.IdFieldName}{Environment.NewLine}");
            sb.Append($"{SQLConstants.FROM} {_sqlConstants.TableFullName}{Environment.NewLine}");

            if (isIncludeDeleted)
            {
                sb.Append($"{SQLConstants.WHERE} {_entityUtils.GetFieldNameFromPropertyName(nameof(BaseEntity.IsDeleted))} = true");
            }
            else
            {
                sb.Append($"{SQLConstants.WHERE} {_entityUtils.GetFieldNameFromPropertyName(nameof(BaseEntity.IsDeleted))} = false");
            }

            var parameters = new List<NpgsqlParameter>();

            if (where != null)
            {
                var prmDictionary = new Dictionary<string, DbParameterInfo>();
                var conditions = _expressionUtils.GetConditions(where.Body, prmDictionary);
                AddToParameters(parameters, prmDictionary);

                sb.Append($" AND {conditions}");
            }

            var result = await SQLExecutor.ExecuteSqlReturningList<long>(sb.ToString(), parameters);
            return result;
        }

        public override async Task<List<T>> SelectAll(Expression<Func<T, bool>> @where, Expression<Func<T, object>> orderByColumn = null, bool isAscending = true, bool isIncludeDeleted = false)
        {
            var sb = new StringBuilder();
            sb.Append($"{SQLConstants.SELECT} *{Environment.NewLine}");
            sb.Append($"{SQLConstants.FROM} {_sqlConstants.TableFullName}{Environment.NewLine}");

            var orderColumn = _sqlConstants.IdFieldName;
            if (orderByColumn != null)
            {
                orderColumn = _expressionUtils.GetField(orderByColumn.Body);
            }

            var ascOrDesc = SQLConstants.DESC;
            if (isAscending)
            {
                ascOrDesc = SQLConstants.ASC;
            }

            var parameters = new List<NpgsqlParameter>();

            AppendWhere(where, parameters, sb, isIncludeDeleted);
            sb.Append($"{SQLConstants.ORDER_BY} {orderColumn} {ascOrDesc}{Environment.NewLine}");

            var items = await SQLExecutor.ExecuteSqlReturningEntityList<T>(sb.ToString(), parameters);
            return items;
        }
    }
}