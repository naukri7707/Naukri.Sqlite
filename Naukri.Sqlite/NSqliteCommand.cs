using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Naukri.Sqlite
{
    public class NSqliteCommand<TTable> : ICommand, IEntry<TTable>, IInsert, ISelect<TTable>, IUpdate<TTable>, IDelete<TTable>, IDistinct<TTable>, IWhere<TTable>, IGroupBy<TTable>, IHaving<TTable>, IOrderBy, ILimit, IExecuteQuery, IExecuteNonQuery
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        #region -- InnerSqliteCommand

        protected readonly SqliteCommand sqliteCommand;

        public int CommandTimeout { get => sqliteCommand.CommandTimeout; set => sqliteCommand.CommandTimeout = value; }

        public SqliteConnection Connection { get => sqliteCommand.Connection; set => sqliteCommand.Connection = value; }

        #endregion

        private readonly NSqliteField[] sqliteFields;

        private readonly StringBuilder commandBuilder;

        public string CommandText => commandBuilder.ToString();

        public string TableName { get; }

        public NSqliteCommand() : this(null, null, null) { }

        public NSqliteCommand(string commandText) : this(commandText, null, null) { }

        public NSqliteCommand(SqliteConnection connection) : this(null, connection, null) { }

        public NSqliteCommand(string commandText, SqliteConnection connection) : this(commandText, connection, null) { }

        public NSqliteCommand(string commandText, SqliteConnection connection, SqliteTransaction transaction)
        {
            sqliteCommand = new SqliteCommand(null, connection, transaction);
            commandBuilder = new StringBuilder(commandText);
            // 取得資料表架構
            var schemaType = typeof(TTable);
            var tableAttr = schemaType.GetCustomAttribute<SqliteTableAttribute>();
            if (tableAttr is null)
            {
                throw new Exception($"{typeof(TTable).Name} 需要有 Table 標籤 e.g. [SqliteTable(\"{typeof(TTable).Name}\")]");
            }
            TableName = tableAttr.Name;
            // 取得有效的欄位資訊
            var props = schemaType.GetProperties(BINDING_FLAGS);
            sqliteFields = new NSqliteField[props.Length];
            int len = 0;
            foreach (var prop in props)
            {
                if (prop.GetCustomAttribute<SqliteFieldAttribute>() != null)
                {
                    sqliteFields[len++] = new NSqliteField(prop);
                }
            }
            Array.Resize(ref sqliteFields, len);
        }

        private NSqliteCommand(SqliteCommand sqliteCommand)
        {
            this.sqliteCommand = sqliteCommand;
            commandBuilder = new StringBuilder(sqliteCommand.CommandText);
        }

        public static implicit operator SqliteCommand(NSqliteCommand<TTable> command) => command.sqliteCommand;

        public static implicit operator NSqliteCommand<TTable>(SqliteCommand command) => new NSqliteCommand<TTable>(command);

        private IInsert InsertOrReplace(string command, TTable row)
        {
            commandBuilder
             .Append($"{command} INTO {TableName} (")
             .AppendArray(sqliteFields, ", ")
             .Append(") VALUES (")
             .AppendArray(sqliteFields, f =>
             {
                 var res = f.GetValueText(row, out object blob);
                 if (Serialize(blob, out byte[] data)) // 處理 BLOB 物件
                 {
                     sqliteCommand.Prepare();
                     sqliteCommand.Parameters.Add(res, DbType.Binary, data.Length);
                     sqliteCommand.Parameters[res].Value = data;
                 }
                 return res;
             }, ", ")
             .Append(");");
            return this;
        }

        public IInsert Insert(TTable row)
            => InsertOrReplace("INSERT", row);

        public IInsert InsertOrReplace(TTable row)
            => InsertOrReplace("REPLACE", row);

        public ISelect<TTable> Select(params dynamic[] columns)
        {
            throw new NotImplementedException();
        }

        public IUpdate<TTable> Update(TTable row)
        {
            throw new NotImplementedException();
        }

        public IDelete<TTable> Delete()
        {
            throw new NotImplementedException();
        }

        public IDelete<TTable> Delete(TTable row)
        {
            throw new NotImplementedException();
        }

        IDistinct<TTable> IDistinctable<TTable>.Distinct()
        {
            throw new NotImplementedException();
        }

        IWhere<TTable> IWhereable<TTable, IWhere<TTable>>.Where(Expression<Func<TTable, bool>> expression)
        {
            throw new NotImplementedException();
        }

        ICondition<IWhere<TTable>> IWhereable<TTable, IWhere<TTable>>.Where<T>(T column)
        {
            throw new NotImplementedException();
        }

        IGroupBy<TTable> IGroupByable<TTable>.GroupBy(params dynamic[] columns)
        {
            throw new NotImplementedException();
        }

        IGroupBy<TTable> IGroupByable<TTable>.GroupBy(Func<TTable, dynamic> func)
        {
            throw new NotImplementedException();
        }

        IOrderBy IOrderByable<TTable>.OrderByAsc(params dynamic[] columns)
        {
            throw new NotImplementedException();
        }

        IOrderBy IOrderByable<TTable>.OrderByDesc(params dynamic[] columns)
        {
            throw new NotImplementedException();
        }

        IOrderBy IOrderByable<TTable>.OrderBy(Func<TTable, dynamic> func, int sortBy)
        {
            throw new NotImplementedException();
        }

        ILimit ILimitable.Limit(int count, int offset)
        {
            throw new NotImplementedException();
        }

        void IExecuteQueryable.ExecuteReader(Action<SqliteDataReader> action)
        {
            throw new NotImplementedException();
        }

        T IExecuteQueryable.ExecuteReader<T>(Func<SqliteDataReader, T> func)
        {
            throw new NotImplementedException();
        }

        T IExecuteQueryable.ExecuteScalar<T>()
        {
            throw new NotImplementedException();
        }

        IExecuteNonQuery IWhereable<TTable, IExecuteNonQuery>.Where(Expression<Func<TTable, bool>> expression)
        {
            throw new NotImplementedException();
        }

        ICondition<IExecuteNonQuery> IWhereable<TTable, IExecuteNonQuery>.Where<T>(T column)
        {
            throw new NotImplementedException();
        }

        int IExecuteNonQueryable.ExecuteNonQuery()
        {
            sqliteCommand.CommandText = CommandText;
            return sqliteCommand.ExecuteNonQuery();
        }

        IHaving<TTable> IHavingable<TTable>.Having()
        {
            throw new NotImplementedException();
        }

        SqliteDataReader IExecuteQueryable.ExecuteReader()
        {
            throw new NotImplementedException();
        }

        public bool Serialize<T>(T obj, out byte[] binary)
        {
            if (obj == null)
            {
                binary = null;
                return false;
            }
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                binary = ms.ToArray();
                return true;
            }
        }

        public bool Deserialize<T>(byte[] binary, out T obj)
        {
            if (binary == null)
            {
                obj = default;
                return false;
            }
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream(binary))
            {
                obj = (T)bf.Deserialize(ms);
                return true;
            }
        }
    }
}

