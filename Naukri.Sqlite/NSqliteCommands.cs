using Mono.Data.Sqlite;
using System;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Naukri.Sqlite
{
    public interface ICommandable
    {
        string CommandText { get; }
    }

    public interface IInsertable<Table>
    {
        IInsert Insert(Table data);

        IInsert Insert(object data);

        IInsert InsertOrReplace(Table data);

        IInsert InsertOrReplace(object data);
    }

    public interface ISelectable<Table>
    {
        ISelect<Table> SelectAll();

        ISelect<Table> Select(object fields);
    }

    public interface IUpdateable<Table>
    {
        IUpdate<Table> Update(Table data);

        IUpdate<Table> Update(object data);
    }

    public interface IDeleteable<Table>
    {
        IDelete<Table> Delete();
    }

    public interface IDistinctable<Table>
    {
        IDistinct<Table> Distinct();
    }

    public interface IWhereable<Table, TResult>
    {
        TResult Where(Expression<Func<bool>> expression);

        TResult Where(Expression<Func<Table, bool>> expression);
    }

    public interface IGroupByable<Table>
    {
        IGroupBy<Table> GroupBy(object fields);
    }

    public interface IHavingable<Table>
    {
        IHaving<Table> Having(Expression<Func<bool>> expression);

        IHaving<Table> Having(Expression<Func<Table, bool>> expression);
    }

    public interface IOrderByable<Table>
    {
        IOrderBy OrderBy(object fields, int sortBy = 1);
    }

    public interface ILimitable
    {
        ILimit Limit(int count);

        ILimit Limit(int count, int offset);
    }

    public interface IExecuteNonQueryable
    {
        int ExecuteNonQuery();

        Task<int> ExecuteNonQueryAsync();
    }

    public interface IExecuteQueryable
    {
        SqliteDataReader ExecuteReader();

        Task<DbDataReader> ExecuteReaderAsync();

        object ExecuteScalar();

        Task<object> ExecuteScalarAsync();
    }

    public interface ICommand : ICommandable { }

    public interface IEntry<Table> : ICommandable, IInsertable<Table>, ISelectable<Table>, IUpdateable<Table>, IDeleteable<Table> { }

    public interface IInsert : ICommand, IExecuteNonQueryable { }

    public interface ISelect<Table> : ICommand, IDistinctable<Table>, IWhereable<Table, IWhere<Table>>, IGroupByable<Table>, IOrderByable<Table>, ILimitable, IExecuteQueryable { }

    public interface IUpdate<Table> : ICommand, IWhereable<Table, IExecuteNonQuery>, IExecuteNonQueryable { }

    public interface IDelete<Table> : ICommand, IWhereable<Table, IExecuteNonQuery>, IExecuteNonQueryable { }

    public interface IDistinct<Table> : ICommand, IWhereable<Table, IWhere<Table>>, IGroupByable<Table>, IOrderByable<Table>, ILimitable, IExecuteQueryable { }

    public interface IWhere<Table> : ICommand, IGroupByable<Table>, IOrderByable<Table>, ILimitable, IExecuteQueryable { }

    public interface IGroupBy<Table> : ICommand, IHavingable<Table>, IOrderByable<Table>, ILimitable, IExecuteQueryable { }

    public interface IHaving<Table> : ICommand, IOrderByable<Table>, ILimitable, IExecuteQueryable { }

    public interface IOrderBy : ICommand, ILimitable, IExecuteQueryable { }

    public interface ILimit : ICommand, IExecuteQueryable { };

    public interface IExecuteQuery : ICommand, IExecuteQueryable { }

    public interface IExecuteNonQuery : ICommand, IExecuteNonQueryable { }

}
