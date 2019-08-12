using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading.Tasks;

using StandardRepository.Factories;
using StandardRepository.Models.Entities;

namespace StandardRepository
{
    public interface IStandardRepository<T, TConnection>
    where T : BaseEntity
    where TConnection : DbConnection
    {
        /// <summary>
        /// When you want to run a group of repositories in one transaction (in unit of works),
        /// you need to set the same transaction for SQL executor
        /// and to do that you need to pass connection at the beginning of the scope.
        /// </summary>
        /// <param name="connection"></param>
        void SetSqlExecutorForTransaction(TConnection connection);

        Task<long> Insert(long currentUserId, T entity);

        /// <summary>
        /// Just runs a foreach... 
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task InsertBulk(long currentUserId, IEnumerable<T> entities);

        /// <summary>
        /// Imports content of the csv to table
        /// The file is looked for in bulk import jobs list
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="bulkImportJobUid"></param>
        /// <returns></returns>
        Task<int> InsertBulk(long currentUserId, Guid bulkImportJobUid);

        Task<bool> Update(long currentUserId, T entity);
        Task<bool> UpdateField(long currentUserId, long id, string fieldName, object value);

        Task<bool> Delete(long currentUserId, long id);
        Task<bool> UndoDelete(long currentUserId, long id);
        Task<bool> HardDelete(long currentUserId, long id);

        /// <summary>
        /// Returns even the record is soft deleted (means no IsDeleted filter)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<T> SelectById(long id);
        Task<T> Select(Expression<Func<T, bool>> where, bool isIncludeDeleted = false);
        Task<List<T>> SelectMany(IEnumerable<long> ids, bool isIncludeDeleted = false);
        Task<List<T>> SelectMany(Expression<Func<T, bool>> where, int skip = 0, int take = 100, Expression<Func<T, object>> orderColumn = null, bool isAscending = true, bool isIncludeDeleted = false, Expression<Func<T, object>> distinctByColumn = null);
        Task<List<T>> SelectAfter(Expression<Func<T, bool>> where, long lastId, int take = 100, Expression<Func<T, object>> orderByColumn = null, bool isAscending = true, bool isIncludeDeleted = false, Expression<Func<T, object>> distinctByColumn = null);
        Task<List<T>> SelectAfter(Expression<Func<T, bool>> where, Guid lastUid, int take = 100, Expression<Func<T, object>> orderByColumn = null, bool isAscending = true, bool isIncludeDeleted = false, Expression<Func<T, object>> distinctByColumn = null);
        Task<List<long>> SelectIds(Expression<Func<T, bool>> where, bool isIncludeDeleted = false);
        Task<List<T>> SelectAll(Expression<Func<T, bool>> where, Expression<Func<T, object>> orderByColumn = null, bool isAscending = true, bool isIncludeDeleted = false);

        Task<List<EntityRevision<T>>> SelectRevisions(long id);
        Task<bool> RestoreRevision(long currentUserId, long id, int revision);

        Task<bool> Any(Expression<Func<T, bool>> where = null, bool isIncludeDeleted = false);
        Task<int> Count(Expression<Func<T, bool>> where = null, bool isIncludeDeleted = false);

        Task<long> Max(Expression<Func<T, bool>> where = null, Expression<Func<T, long>> maxColumn = null, bool isIncludeDeleted = false);
        Task<int> Max(Expression<Func<T, bool>> where = null, Expression<Func<T, int>> maxColumn = null, bool isIncludeDeleted = false);
        Task<decimal> Max(Expression<Func<T, bool>> where = null, Expression<Func<T, decimal>> maxColumn = null, bool isIncludeDeleted = false);

        Task<long> Min(Expression<Func<T, bool>> where = null, Expression<Func<T, long>> minColumn = null, bool isIncludeDeleted = false);
        Task<int> Min(Expression<Func<T, bool>> where = null, Expression<Func<T, int>> minColumn = null, bool isIncludeDeleted = false);
        Task<decimal> Min(Expression<Func<T, bool>> where = null, Expression<Func<T, decimal>> minColumn = null, bool isIncludeDeleted = false);

        Task<long> Sum(Expression<Func<T, bool>> where = null, Expression<Func<T, long>> sumColumn = null, bool isIncludeDeleted = false);
        Task<int> Sum(Expression<Func<T, bool>> where = null, Expression<Func<T, int>> sumColumn = null, bool isIncludeDeleted = false);
        Task<decimal> Sum(Expression<Func<T, bool>> where = null, Expression<Func<T, decimal>> sumColumn = null, bool isIncludeDeleted = false);
    }
}