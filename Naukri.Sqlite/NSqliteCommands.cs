using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

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

        ISelect<Table> Count();
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
        IOrderBy<Table> OrderBy(object fields, int sortBy = 1);
    }

    public interface ILimitable<Table>
    {
        ILimit<Table> Limit(int count);

        ILimit<Table> Limit(int count, int offset);
    }

    public interface IExecuteNonQueryable
    {
        int ExecuteNonQuery();
    }

    public interface IExecuteQueryable
    {
        SqliteDataReader ExecuteReader();

        object ExecuteScalar();
    }

    public interface IEntry<Table> : IInsertable<Table>, ISelectable<Table>, IUpdateable<Table>, IDeleteable<Table> { }

    public interface ICommand : ICommandable { }

    public interface IInsert : ICommand, IExecuteNonQueryable { }

    public interface ISelect<Table> : ICommand, IEnumerable<Table>, IDistinctable<Table>, IWhereable<Table, IWhere<Table>>,
                     IGroupByable<Table>, IOrderByable<Table>, ILimitable<Table>, IExecuteQueryable { }

    public interface IUpdate<Table> : ICommand, IWhereable<Table, IExecuteNonQuery>, IExecuteNonQueryable { }

    public interface IDelete<Table> : ICommand, IWhereable<Table, IExecuteNonQuery>, IExecuteNonQueryable { }

    public interface IDistinct<Table> : ICommand, IEnumerable<Table>, IWhereable<Table, IWhere<Table>>, IGroupByable<Table>,
                     IOrderByable<Table>, ILimitable<Table>, IExecuteQueryable { }

    public interface IWhere<Table> : ICommand, IEnumerable<Table>, IGroupByable<Table>, IOrderByable<Table>, ILimitable<Table>, IExecuteQueryable { }

    public interface IGroupBy<Table> : ICommand, IEnumerable<Table>, IHavingable<Table>, IOrderByable<Table>, ILimitable<Table>, IExecuteQueryable { }

    public interface IHaving<Table> : ICommand, IEnumerable<Table>, IOrderByable<Table>, ILimitable<Table>, IExecuteQueryable { }

    public interface IOrderBy<Table> : ICommand, IEnumerable<Table>, ILimitable<Table>, IExecuteQueryable { }

    public interface ILimit<Table> : ICommand, IEnumerable<Table>, IExecuteQueryable { };

    public interface IExecute : ICommand, IExecuteQueryable, IExecuteNonQueryable { }

    public interface IExecuteQuery : ICommand, IExecuteQueryable { }

    public interface IExecuteNonQuery : ICommand, IExecuteNonQueryable { }
}
