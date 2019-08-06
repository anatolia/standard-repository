using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using StandardRepository.Helpers;
using StandardRepository.Helpers.SqlExecutor;
using StandardRepository.Models.Entities;

namespace StandardRepository
{
    public abstract class StandardRepository<T, TConnection, TCommand, TParameter> : IStandardRepository<T, TConnection>
    where T : BaseEntity, new()
    where TConnection : DbConnection, new()
    where TCommand : DbCommand, new()
    where TParameter : DbParameter, new()
    {
        protected string QueryInsert { get; set; }
        protected string QueryUpdate { get; set; }
        protected string QueryDelete { get; set; }
        protected string QueryUndoDelete { get; set; }
        protected string QueryHardDelete { get; set; }
        protected string QuerySelectById { get; set; }
        protected string QuerySelectRevisions { get; set; }
        protected string QueryRestoreRevision { get; set; }

        public List<string> UpdateableFields { get; protected set; }

        protected PropertyInfo[] Fields { get; }
        protected PropertyInfo[] BaseFields { get; }

        protected readonly TypeLookup _typeLookup;
        protected readonly EntityUtils _entityUtils;
        protected readonly SQLConstants _sqlConstants;
        protected readonly ExpressionUtils _expressionUtils;

        protected StandardRepository(TypeLookup typeLookup, SQLConstants sqlConstants, EntityUtils entityUtils, ExpressionUtils expressionUtils,
                                     ISQLExecutor<TCommand, TConnection, TParameter> sqlExecutor, List<string> updateableFields = null)
        {
            _typeLookup = typeLookup;
            _sqlConstants = sqlConstants;
            _entityUtils = entityUtils;
            _expressionUtils = expressionUtils;
            SQLExecutor = sqlExecutor;
            UpdateableFields = updateableFields;

            var entityType = typeof(T);
            Fields = _entityUtils.GetProperties(entityType);
            BaseFields = _entityUtils.GetBaseProperties();
        }

        public ISQLExecutor<TCommand, TConnection, TParameter> SQLExecutor { get; set; }
        public abstract void SetSqlExecutorForTransaction(TConnection connection);

        public async Task<long> Insert(long currentUserId, T entity)
        {
            var resultParameters = new List<TParameter>();
            SQLExecutor.AddParameter(resultParameters, SQLConstants.UPDATED_BY_PARAMETER_NAME, currentUserId, DbType.Int64);
            SQLExecutor.AddParameter(resultParameters, SQLConstants.UID_PARAMETER_NAME, entity.Uid, DbType.Guid);
            SQLExecutor.AddParameter(resultParameters, SQLConstants.NAME_PARAMETER_NAME, entity.Name, DbType.String);

            for (var i = 0; i < Fields.Length; i++)
            {
                var field = Fields[i];
                if (BaseFields.Any(x => x.Name == field.Name))
                {
                    continue;
                }

                var parameter = GetParameterInfoFromField(entity, field);
                resultParameters.AddRange(parameter);
            }

            var id = await SQLExecutor.ExecuteSqlReturningValue<long>(QueryInsert, resultParameters);
            entity.Id = id;
            return id;
        }

        public async Task InsertBulk(long currentUserId, IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                await Insert(currentUserId, entity);
            }
        }

        public Task<int> InsertBulk(long currentUserId, Guid bulkImportJobUid)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Update(long currentUserId, T entity)
        {
            if (entity.Id < 1)
            {
                throw new ArgumentException("entity does not have id!");
            }

            var commandParameters = GetParametersForIdAndCurrentUser(currentUserId, entity.Id);
            SQLExecutor.AddParameter(commandParameters, SQLConstants.NAME_PARAMETER_NAME, entity.Name, DbType.String);

            for (var i = 0; i < Fields.Length; i++)
            {
                var field = Fields[i];
                var parameters = GetParameterInfoFromField(entity, field);
                commandParameters.AddRange(parameters);
            }

            await SQLExecutor.ExecuteSql(QueryUpdate, commandParameters);
            return true;
        }

        public async Task<bool> UpdateField(long currentUserId, long id, string fieldName, object value)
        {
            if (!UpdateableFields.Any()
                || UpdateableFields.All(x => x != fieldName))
            {
                throw new SecurityException("updateable fields not defined for entity");
            }

            var prmName = _sqlConstants.ParameterSign + _entityUtils.GetFieldNameFromPropertyName(fieldName);
            var parameters = GetParametersForId(id);
            parameters.Add(SQLExecutor.CreateParameter(prmName, value));

            var updateSql = $@"{SQLConstants.UPDATE} {_sqlConstants.TableFullName} 
                               {SQLConstants.SET} {fieldName} = {prmName}
                               {SQLConstants.WHERE}  {_sqlConstants.IdFieldName} = {_sqlConstants.ParameterSign}{_sqlConstants.IdParameterName}";

            await SQLExecutor.ExecuteSql(updateSql, parameters);
            return true;
        }

        public async Task<bool> Delete(long currentUserId, long id)
        {
            var parameters = GetParametersForIdAndCurrentUser(currentUserId, id);
            await SQLExecutor.ExecuteSql(QueryDelete, parameters);
            return true;
        }

        public async Task<bool> UndoDelete(long currentUserId, long id)
        {
            var parameters = GetParametersForCurrentUserIdAndId(currentUserId, id);
            await SQLExecutor.ExecuteSql(QueryUndoDelete, parameters);

            return true;
        }

        public async Task<bool> HardDelete(long currentUserId, long id)
        {
            var parameters = GetParametersForCurrentUserIdAndId(currentUserId, id);
            await SQLExecutor.ExecuteSql(QueryHardDelete, parameters);

            return true;
        }

        public async Task<T> SelectById(long id)
        {
            var parameters = GetParametersForId(id);
            var result = await SQLExecutor.ExecuteSqlReturningEntity<T>(QuerySelectById, parameters);
            return result;
        }

        protected abstract void AppendWhere(Expression<Func<T, bool>> where, List<TParameter> parameters, StringBuilder sb, bool isIncludeDeleted);

        public async Task<T> Select(Expression<Func<T, bool>> @where, bool isIncludeDeleted = false)
        {
            var sb = new StringBuilder();
            sb.Append($"{SQLConstants.SELECT} *{Environment.NewLine}");
            sb.Append($"{SQLConstants.FROM} {_sqlConstants.TableFullName}{Environment.NewLine}");

            var parameters = new List<TParameter>();
            AppendWhere(where, parameters, sb, isIncludeDeleted);

            var result = await SQLExecutor.ExecuteSqlReturningEntity<T>(sb.ToString(), parameters);
            return result;
        }

        public async Task<List<T>> SelectMany(IEnumerable<long> ids, bool isIncludeDeleted = false)
        {
            var idList = ids as long[] ?? ids.ToArray();
            if (!idList.Any())
            {
                return new List<T>();
            }

            var sb = new StringBuilder();
            sb.Append($"{SQLConstants.SELECT} *{Environment.NewLine}");
            sb.Append($"{SQLConstants.FROM} {_sqlConstants.TableFullName}{Environment.NewLine}");
            sb.Append($"{SQLConstants.WHERE} ");

            if (!isIncludeDeleted)
            {
                sb.Append($"{_entityUtils.GetFieldNameFromPropertyName(nameof(BaseEntity.IsDeleted))} = false AND ");
            }

            var idsInString = string.Join("::bigint,", idList);
            sb.Append($"{_sqlConstants.IdFieldName} IN ({idsInString}){Environment.NewLine}");

            var result = await SQLExecutor.ExecuteSqlReturningEntityList<T>(sb.ToString(), null);
            return result;
        }

        public abstract Task<List<T>> SelectMany(Expression<Func<T, bool>> where, int skip = 0, int take = 100,
                                                 Expression<Func<T, object>> orderByColumn = null, bool isAscending = true, bool isIncludeDeleted = false);

        public abstract Task<List<T>> SelectAfter(Expression<Func<T, bool>> where, long lastId, int take = 100,
                                                  Expression<Func<T, object>> orderByColumn = null, bool isAscending = true, bool isIncludeDeleted = false);

        public abstract Task<List<T>> SelectAfter(Expression<Func<T, bool>> where, Guid lastUid, int take = 100,
                                                  Expression<Func<T, object>> orderByColumn = null, bool isAscending = true, bool isIncludeDeleted = false);

        public abstract Task<List<long>> SelectIds(Expression<Func<T, bool>> where, bool isIncludeDeleted = false);

        public abstract Task<List<T>> SelectAll(Expression<Func<T, bool>> where, Expression<Func<T, object>> orderByColumn = null,
                                                bool isAscending = true, bool isIncludeDeleted = false);

        public async Task<List<EntityRevision<T>>> SelectRevisions(long id)
        {
            var parameters = GetParametersForId(id);

            var result = await SQLExecutor.ExecuteSqlReturningRevisionList<T>(QuerySelectRevisions, parameters);
            return result;
        }

        public async Task<bool> RestoreRevision(long currentUserId, long id, int revision)
        {
            var parameters = GetParametersForIdAndCurrentUser(currentUserId, id);
            SQLExecutor.AddParameter(parameters, SQLConstants.REVISION_PARAMETER_NAME, revision, DbType.Int32);

            var result = await SQLExecutor.ExecuteSqlReturningValue<int>(QueryRestoreRevision, parameters);
            return result == 1;
        }

        public async Task<bool> Any(Expression<Func<T, bool>> @where = null, bool isIncludeDeleted = false)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{SQLConstants.SELECT} 1");
            sb.AppendLine($"{SQLConstants.FROM} {_sqlConstants.TableFullName}");

            var parameters = new List<TParameter>();
            AppendWhere(where, parameters, sb, isIncludeDeleted);

            var result = await SQLExecutor.ExecuteSqlReturningValue<bool>(sb.ToString(), parameters);
            return result;
        }

        public async Task<int> Count(Expression<Func<T, bool>> @where = null, bool isIncludeDeleted = false)
        {
            var sb = new StringBuilder();
            sb.Append($"{SQLConstants.SELECT} {SQLConstants.COUNT}(*){Environment.NewLine}");
            sb.Append($"{SQLConstants.FROM} {_sqlConstants.TableFullName}{Environment.NewLine}");

            var parameters = new List<TParameter>();
            AppendWhere(where, parameters, sb, isIncludeDeleted);

            var result = await SQLExecutor.ExecuteSqlReturningValue<int>(sb.ToString(), parameters);
            return result;
        }

        public async Task<long> Max(Expression<Func<T, bool>> @where = null, Expression<Func<T, long>> maxColumn = null, bool isIncludeDeleted = false)
        {
            if (maxColumn == null)
            {
                throw new ArgumentException("the field to get the max is not specified!");
            }

            var maxColumnField = _expressionUtils.GetField(maxColumn.Body);

            var parameters = new List<TParameter>();
            var sb = GetMaxColumnQuery(where, maxColumnField, parameters);

            var result = await SQLExecutor.ExecuteSqlReturningValue<long>(sb, parameters);
            return result;
        }

        public async Task<int> Max(Expression<Func<T, bool>> @where = null, Expression<Func<T, int>> maxColumn = null, bool isIncludeDeleted = false)
        {
            if (maxColumn == null)
            {
                throw new ArgumentNullException(nameof(maxColumn));
            }

            var maxColumnField = _expressionUtils.GetField(maxColumn.Body);

            var parameters = new List<TParameter>();
            var sb = GetMaxColumnQuery(where, maxColumnField, parameters);

            var result = await SQLExecutor.ExecuteSqlReturningValue<int>(sb, parameters);
            return result;
        }

        public async Task<decimal> Max(Expression<Func<T, bool>> @where = null, Expression<Func<T, decimal>> maxColumn = null, bool isIncludeDeleted = false)
        {
            if (maxColumn == null)
            {
                throw new ArgumentNullException(nameof(maxColumn));
            }

            var maxColumnField = _expressionUtils.GetField(maxColumn.Body);

            var parameters = new List<TParameter>();
            var sb = GetMaxColumnQuery(where, maxColumnField, parameters);

            var result = await SQLExecutor.ExecuteSqlReturningValue<decimal>(sb, parameters);
            return result;
        }

        public async Task<long> Min(Expression<Func<T, bool>> @where = null, Expression<Func<T, long>> minColumn = null, bool isIncludeDeleted = false)
        {
            if (minColumn == null)
            {
                throw new ArgumentException("the field to get the min is not specified!");
            }

            var minColumnField = _expressionUtils.GetField(minColumn.Body);

            var parameters = new List<TParameter>();
            var sb = GetMinColumnQuery(where, minColumnField, parameters);

            var result = await SQLExecutor.ExecuteSqlReturningValue<long>(sb, parameters);
            return result;
        }

        public async Task<int> Min(Expression<Func<T, bool>> @where = null, Expression<Func<T, int>> minColumn = null, bool isIncludeDeleted = false)
        {
            if (minColumn == null)
            {
                throw new ArgumentException("the field to get the min is not specified!");
            }

            var minColumnField = _expressionUtils.GetField(minColumn.Body);

            var parameters = new List<TParameter>();
            var sb = GetMinColumnQuery(where, minColumnField, parameters);

            var result = await SQLExecutor.ExecuteSqlReturningValue<int>(sb, parameters);
            return result;
        }

        public async Task<decimal> Min(Expression<Func<T, bool>> @where = null, Expression<Func<T, decimal>> minColumn = null, bool isIncludeDeleted = false)
        {
            if (minColumn == null)
            {
                throw new ArgumentException("the field to get the min is not specified!");
            }

            var minColumnField = _expressionUtils.GetField(minColumn.Body);

            var parameters = new List<TParameter>();
            var sb = GetMinColumnQuery(where, minColumnField, parameters);

            var result = await SQLExecutor.ExecuteSqlReturningValue<decimal>(sb, parameters);
            return result;
        }

        public async Task<long> Sum(Expression<Func<T, bool>> @where = null, Expression<Func<T, long>> sumColumn = null, bool isIncludeDeleted = false)
        {
            if (sumColumn == null)
            {
                throw new ArgumentException("the field to get the sum is not specified!");
            }

            var sumColumnField = _expressionUtils.GetField(sumColumn.Body);

            var parameters = new List<TParameter>();
            var sb = GetSumColumnQuery(where, sumColumnField, parameters);

            var result = await SQLExecutor.ExecuteSqlReturningValue<long>(sb, parameters);
            return result;
        }

        public async Task<int> Sum(Expression<Func<T, bool>> @where = null, Expression<Func<T, int>> sumColumn = null, bool isIncludeDeleted = false)
        {
            if (sumColumn == null)
            {
                throw new ArgumentException("the field to get the sum is not specified!");
            }

            var sumColumnField = _expressionUtils.GetField(sumColumn.Body);

            var parameters = new List<TParameter>();
            var sb = GetSumColumnQuery(where, sumColumnField, parameters);

            var result = await SQLExecutor.ExecuteSqlReturningValue<int>(sb, parameters);
            return result;
        }

        public async Task<decimal> Sum(Expression<Func<T, bool>> @where = null, Expression<Func<T, decimal>> sumColumn = null, bool isIncludeDeleted = false)
        {
            if (sumColumn == null)
            {
                throw new ArgumentException("the field to get the sum is not specified!");
            }

            var sumColumnField = _expressionUtils.GetField(sumColumn.Body);

            var parameters = new List<TParameter>();
            var sb = GetSumColumnQuery(where, sumColumnField, parameters);

            var result = await SQLExecutor.ExecuteSqlReturningValue<decimal>(sb, parameters);
            return result;
        }

        #region Helpers
        private TParameter[] GetParameterInfoFromField(T entity, PropertyInfo field)
        {
            var prms = new List<TParameter>();

            var parameterName = _entityUtils.GetParameterNameFromPropertyName(field.Name);
            var dbType = _typeLookup.GetDbType(field.PropertyType);
            var valueFromProperty = GetValueFromProperty(entity, field);

            SQLExecutor.AddParameter(prms, parameterName, valueFromProperty, dbType);

            return prms.ToArray();
        }

        private static object GetValueFromProperty(T entity, PropertyInfo field)
        {
            var property = entity.GetType().GetProperty(field.Name);
            var value = property.GetValue(entity, null);

            if (property.PropertyType == typeof(string)
                && string.IsNullOrWhiteSpace(Convert.ToString(value)))
            {
                return string.Empty;
            }

            if (value == null)
            {
                return DBNull.Value;
            }

            return value;
        }

        private List<TParameter> GetParametersForIdAndCurrentUser(long currentUserId, long id)
        {
            var parameters = GetParametersForId(id);
            SQLExecutor.AddParameter(parameters, SQLConstants.UPDATED_BY_PARAMETER_NAME, currentUserId, DbType.Int64);
            return parameters;
        }

        private List<TParameter> GetParametersForId(long id)
        {
            var parameters = new List<TParameter>();
            SQLExecutor.AddParameter(parameters, _sqlConstants.IdParameterName, id, DbType.Int64);
            return parameters;
        }

        private List<TParameter> GetParametersForCurrentUserIdAndId(long currentUserId, long id)
        {
            var parameters = new List<TParameter>();

            SQLExecutor.AddParameter(parameters, SQLConstants.UPDATED_BY_PARAMETER_NAME, currentUserId, DbType.Int64);
            SQLExecutor.AddParameter(parameters, _sqlConstants.IdParameterName, id, DbType.Int64);

            return parameters;
        }

        private string GetMinColumnQuery(Expression<Func<T, bool>> where, string minColumnField, List<TParameter> parameters, bool isIncludeDeleted = false)
        {
            var sb = new StringBuilder();
            sb.Append($"{SQLConstants.SELECT} {SQLConstants.MIN}({minColumnField}){Environment.NewLine}");
            sb.Append($"{SQLConstants.FROM} {_sqlConstants.TableFullName}{Environment.NewLine}");

            AppendWhere(where, parameters, sb, isIncludeDeleted);

            return sb.ToString();
        }

        private string GetMaxColumnQuery(Expression<Func<T, bool>> where, string maxColumnField, List<TParameter> parameters, bool isIncludeDeleted = false)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{SQLConstants.SELECT} {SQLConstants.MAX}({maxColumnField})");
            sb.AppendLine($"{SQLConstants.FROM} {_sqlConstants.TableFullName}");

            AppendWhere(where, parameters, sb, isIncludeDeleted);

            return sb.ToString();
        }

        private string GetSumColumnQuery(Expression<Func<T, bool>> where, string sumColumnField, List<TParameter> parameters, bool isIncludeDeleted = false)
        {
            var sb = new StringBuilder();
            sb.Append($"{SQLConstants.SELECT} {SQLConstants.SUM}({sumColumnField}){Environment.NewLine}");
            sb.Append($"{SQLConstants.FROM} {_sqlConstants.TableFullName}{Environment.NewLine}");

            AppendWhere(where, parameters, sb, isIncludeDeleted);

            return sb.ToString();
        }

        #endregion
    }
}