using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Ekr.Dapper.Connection.Contracts.Base
{
    public interface IBaseConnection
    {
        Task<T> WithConnectionAsync<T>(Func<IDbConnection, Task<T>> func);
        Task WithConnectionAsync(Func<IDbConnection, Task> func);
        void WithTransactionDapper(Action<IDbConnection, IDbTransaction> actions);
        T WithTransactionDapper<T>(Func<IDbConnection, IDbTransaction, T> func);
        T WithConnection<T>(Func<IDbConnection, T> func);
        void WithConnection(Action<IDbConnection> func);
        void WithTransaction(Action<IDbConnection> actions);
        IEnumerable<T> WithTransaction<T>(Func<IDbConnection, IEnumerable<T>> func);
        T WithTransaction<T>(Func<IDbConnection, T> func);
        Task WithTransactionAsync(Func<IDbConnection, Task> actions);
    }
}