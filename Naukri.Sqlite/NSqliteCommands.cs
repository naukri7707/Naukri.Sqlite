using Mono.Data.Sqlite;
using System;
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
        TResult Where(Expression<Func<Table, bool>> expression);

        ICondition<TResult> Where<T>(T column);
    }

    public interface ICondition<TResult>
    {
        TResult Like(string expression);

        TResult Glob(string expression);

        TResult Between<T>(T min, T max);

        TResult NotBetween<T>(T min, T max);
    }

    public interface IGroupByable<Table>
    {
        IGroupBy<Table> GroupBy(params dynamic[] columns);

        IGroupBy<Table> GroupBy(Func<Table, dynamic> func);
    }

    public interface IHavingable<Table>
    {

        IHaving<Table> Having();
    }

    public interface IOrderByable<Table>
    {
        IOrderBy OrderByAsc(params dynamic[] columns);

        IOrderBy OrderByDesc(params dynamic[] columns);

        IOrderBy OrderBy(Func<Table, dynamic> func, int sortBy);
    }

    public interface ILimitable
    {
        ILimit Limit(int count, int offset = 0);
    }

    public interface IExecuteQueryable
    {
        SqliteDataReader ExecuteReader();

        T ExecuteScalar<T>();

        void ExecuteReader(Action<SqliteDataReader> action);

        T ExecuteReader<T>(Func<SqliteDataReader, T> func);
    }

    public interface IExecuteNonQueryable
    {
        int ExecuteNonQuery();
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

    public interface IExecuteQuery : ICommand { }

    public interface IExecuteNonQuery : ICommand { }

}
