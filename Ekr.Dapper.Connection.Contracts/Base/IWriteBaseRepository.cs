using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ekr.Dapper.Connection.Contracts.Base
{
    public interface IWriteBaseRepository
    {
        long Insert<T>(T entity) where T : new();
        long InsertIncrement<T>(T entity) where T : new();
        void InsertAll<T>(IEnumerable<T> entities) where T : new();
        long Update<T>(T entity) where T : new();
        Task<int> UpdateAsync<T>(T entity) where T : new();
        void Delete<T>(T entity) where T : new();
        void Delete<T>(Expression<Func<T, bool>> where);
        void DeleteFirst<T>(Expression<Func<T, bool>> where) where T : new();
        void DeleteAll<T>(IEnumerable<T> entities) where T : new();

        /// <summary>
        /// Perform bulk insert operation on the given entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        void BulkInsert<T>(List<T> entities) where T : new();
    }

    public interface IReadBaseRepository
    {
        /// <summary>
        /// Get First or Default from given predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        T GetFirstOrDefault<T>(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Get Last insert (long) Id from last operation, just work with integer (or long) operation
        /// </summary>
        /// <returns> Last ID</returns>
        long GetLastInsertId();

        /// <summary>
        /// Implement pagination from the given query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"> query that need to be executed</param>
        /// <param name="orderBy">order by query without the "order by"</param>
        /// <param name="pageSize"> max size that need to be fetch</param>
        /// <param name="pageNumber"> number of page that want to be fetch by give page size</param>
        /// <returns></returns>
        IEnumerable<T> WithPagination<T>(string query, string orderBy, int pageSize, int pageNumber);

        /// <summary>
        /// Implement pagination from the given query, BEWARE that the query needs to have ORDER BY clause in it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">query that need to be executed</param>
        /// <param name="pageSize">max size that need to be fetch</param>
        /// <param name="pageNumber">number of page that want to be fetch by give page size</param>
        /// <returns></returns>
        IEnumerable<T> WithPagination<T>(string query, int pageSize, int pageNumber);
    }
}
