using Dapper;
using Ekr.Core.Helper;
using Ekr.Dapper.Connection.Contracts.Base;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ekr.Repository
{
    public class BaseRepository : IBaseRepository
    {
        protected BaseRepository(IBaseConnection conn)
        {
            Db = conn;
            //LoadCache();
        }

        protected readonly IBaseConnection Db;

        public virtual long Insert<T>(T entity) where T : new()
        {
            return Db.WithConnection(c => c.Insert(entity));
        }

        public virtual long InsertIncrement<T>(T entity) where T : new()
        {
            return Db.WithConnection(c => c.Insert(entity, true));
        }

        public virtual void InsertAll<T>(IEnumerable<T> entities) where T : new()
        {
            Db.WithConnection(c => c.InsertAll(entities));
        }

        public virtual long Update<T>(T entity) where T : new()
        {
            return Db.WithConnection(c => c.Update(entity));
        }

        public virtual Task<int> UpdateAsync<T>(T entity) where T : new()
        {
            return Db.WithConnectionAsync(c => c.UpdateAsync(entity));
        }

        /// <summary>
        /// Perform bulk insert operation on the given entities
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public virtual void BulkInsert<T>(List<T> entities) where T : new()
        {
            Db.WithConnection(c => DbBulkActionUtilities.BulkInsert(c.ConnectionString, typeof(T).Name, entities));
        }

        public virtual void Delete<T>(T entity) where T : new()
        {
            Db.WithConnection(c => c.Delete(entity));
        }

        public virtual void Delete<T>(Expression<Func<T, bool>> where)
        {
            Db.WithConnection(c => c.Delete(where));
        }

        public virtual void DeleteFirst<T>(Expression<Func<T, bool>> where) where T : new()
        {
            Db.WithConnection(c =>
            {
                var data = c.Select(where).FirstOrDefault();
                if (data == null) return;
                c.Delete(data);
            });
        }

        public virtual void DeleteAll<T>(IEnumerable<T> entities) where T : new()
        {
            Db.WithConnection(c => c.DeleteAll(entities));
        }

        public virtual T GetFirstOrDefault<T>(Expression<Func<T, bool>> predicate)
        {
            return Db.WithConnection(db => db.Select(predicate).FirstOrDefault());
        }

        public virtual long GetLastInsertId()
        {
            //return Db.WithConnection(db => db.GetLastInsertId());
            return 0;
        }

        private const string PaginationQuery = "OFFSET @pageSize * (@pageNumber - 1) ROWS FETCH NEXT @pageSize ROWS ONLY";

        /// <summary>
        /// Implement pagination from the given query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"> query that need to be executed</param>
        /// <param name="orderBy">order by query without the "order by"</param>
        /// <param name="pageSize"> max size that need to be fetch</param>
        /// <param name="pageNumber"> number of page that want to be fetch by give page size</param>
        /// <returns></returns>
        public virtual IEnumerable<T> WithPagination<T>(string query, string orderBy, int pageSize, int pageNumber)
        {
            return
                Db.WithConnection(
                    db => db.Query<T>(query + "ORDER BY " + orderBy + PaginationQuery, new { pageSize, pageNumber }));
        }

        public virtual IEnumerable<T> WithPagination<T>(string query, int pageSize, int pageNumber)
        {
            return
                Db.WithConnection(
                    db => db.Query<T>(query + PaginationQuery, new { pageSize, pageNumber }));
        }
    }
}
