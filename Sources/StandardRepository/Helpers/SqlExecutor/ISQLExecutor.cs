using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

using StandardRepository.Models.Entities;

namespace StandardRepository.Helpers.SqlExecutor
{
    public interface ISQLExecutor<TCommand, TParameter>
    where TCommand : DbCommand, new()
    where TParameter : DbParameter, new()
    {
        TParameter AddParameter<TValue>(TCommand command, string parameterName, TValue parameterValue,
                                        DbType? parameterType = null, int? size = null, byte? precision = null, byte? scale = null);

        TParameter AddParameter<TValue>(DbParameterCollection parameters, string parameterName, TValue parameterValue,
                                        DbType? parameterType = null, int? size = null, byte? precision = null, byte? scale = null);

        TParameter AddParameter<TValue>(ICollection<TParameter> parameters, string parameterName, TValue parameterValue,
                                        DbType? parameterType = null, int? size = null, byte? precision = null, byte? scale = null);

        TParameter CreateParameter<TValue>(string parameterName, TValue parameterValue, DbType? parameterType = null,
                                           int? size = null, byte? precision = null, byte? scale = null);

        Task ExecuteSql(string sql);

        Task<T> ExecuteStoredProcedureReturningValue<T>(string storedProcedureName, IEnumerable<TParameter> parameters = null);

        Task ExecuteStoredProcedure(string storedProcedureName, List<TParameter> parameters = null);

        Task<T> ExecuteStoredProcedureReturningEntity<T>(string storedProcedureName, List<TParameter> parameters = null) where T : BaseEntity, new();

        Task ExecuteSql(string sql, List<TParameter> parameters = null);

        Task<T> ExecuteSqlReturningValue<T>(string sql, List<TParameter> parameters = null);

        Task<T> ExecuteSqlReturningEntity<T>(string sql, List<TParameter> parameters = null) where T : BaseEntity, new();

        Task<List<T>> ExecuteSqlReturningEntityList<T>(string sql, List<TParameter> parameters = null) where T : BaseEntity, new();
        Task<List<T>> ExecuteSqlReturningList<T>(string sql, List<TParameter> parameters = null);

        Task<List<EntityRevision<T>>> ExecuteSqlReturningRevisionList<T>(string sql, List<TParameter> parameters = null) where T : BaseEntity, new();

        List<string> ExecuteSqlForList(string sql);
    }
}