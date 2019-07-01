using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

using StandardRepository.Models.Entities;

namespace StandardRepository.Helpers.SqlExecutor
{
    public interface ISQLExecutor<TCommand, TConnection, TParameter>
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
        Task ExecuteSql(string sql, List<TParameter> parameters);

        Task ExecuteStoredProcedure(string storedProcedureName, List<TParameter> parameters);
        Task<T> ExecuteStoredProcedureReturningValue<T>(string storedProcedureName, IEnumerable<TParameter> parameters);
        Task<T> ExecuteStoredProcedureReturningEntity<T>(string storedProcedureName, List<TParameter> parameters) where T : BaseEntity, new();
        
        Task<T> ExecuteSqlReturningValue<T>(string sql, List<TParameter> parameters);
        Task<List<T>> ExecuteSqlReturningList<T>(string sql, List<TParameter> parameters);
        Task<T> ExecuteSqlReturningEntity<T>(string sql, List<TParameter> parameters) where T : BaseEntity, new();
        Task<List<T>> ExecuteSqlReturningEntityList<T>(string sql, List<TParameter> parameters) where T : BaseEntity, new();
        Task<List<EntityRevision<T>>> ExecuteSqlReturningRevisionList<T>(string sql, List<TParameter> parameters) where T : BaseEntity, new();

        void SetConnection(TConnection connection);
    }
}