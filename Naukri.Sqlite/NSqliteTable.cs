using Mono.Data.Sqlite;
using System;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Naukri.Sqlite
{
    public sealed class NSqliteTable<TTable>
        : IDisposable, IEntry<TTable>, IInsert, ISelect<TTable>, IUpdate<TTable>, IDelete<TTable>, IDistinct<TTable>
        , IWhere<TTable>, IGroupBy<TTable>, IHaving<TTable>, IOrderBy, ILimit, IExecute, IExecuteQuery, IExecuteNonQuery
    {
        SqliteConnection Connection { get; set; }

        SqliteCommand Command { get; set; }

        SqliteDataReader DataReader { get; set; }

        internal NSqliteTableInfo Info { get; }

        public string TableName { get; }

        private readonly NSqliteFieldInfo[] fieldInfos;

        private readonly StringBuilder commandBuilder = new StringBuilder();

        public string CommandText => commandBuilder.ToString();

        public NSqliteTable()
        {
            var info = NSqliteTableInfo.GetTableInfo<TTable>();
            Connection = new SqliteConnection(info.ConnectionText);
            Command = new SqliteCommand(Connection);
            Connection.Open();
            TableName = info.Name;
            fieldInfos = info.FieldInfos;
        }


        private NSqliteFieldInfo[] VerifyAndGetInfos(object data)
        {
            // 取得資料架構
            var type = data.GetType();
            var infos = type.GetProperties(NSqlite.BINDING_FLAGS);
            // 驗證所有屬性皆具有 SqliteField 特性並回傳對應的 NSqliteFieldInfo[]
            var res = new NSqliteFieldInfo[infos.Length];
            for (int i = 0; i < infos.Length; i++)
            {
                int j = fieldInfos.Length;
                // 比對是否有相同的名稱被註冊 (是 NSqliteField)
                while (--j >= 0 && infos[i].Name != fieldInfos[j].Info.Name)
                    ;
                if (j >= 0)
                {
                    res[i] = fieldInfos[j];
                    res[i].Info = infos[i];
                }
                else
                {
                    throw new Exception($"成員\"{infos[i].Name}\" 缺少 [SqliteField] 標籤");
                }
            }
            return res;
        }

        private IInsert InsertCommandBuilder(string command, object data, NSqliteFieldInfo[] fields)
        {
            commandBuilder
            .Append(command, " INTO ", TableName, " (")
            .Append(fields, f => f.Name, ", ")
            .Append(") VALUES (")
            .Append(fields, (Func<NSqliteFieldInfo, string>)(f =>
            {
                var valueText = f.GetValueText(data, out var blob);
                if (Serialize(blob, out byte[] sData)) // 處理 BLOB 物件
                {
                    Command.Prepare();
                    Command.Parameters.Add(valueText, DbType.Binary, sData.Length);
                    Command.Parameters[valueText].Value = sData;
                }
                return valueText;
            }), ", ")
            .Append(")");
            return this;
        }

        private IUpdate<TTable> UpdateCommandBuilder(object data, NSqliteFieldInfo[] fields)
        {
            commandBuilder
            .Append("UPDATE ", TableName, " SET ")
            .Append(fields, f =>
            {
                var valueText = f.GetValueText(data, out var blob);
                if (Serialize(blob, out byte[] sData)) // 處理 BLOB 物件
                {
                    this.Command.Prepare();
                    this.Command.Parameters.Add(valueText, DbType.Binary, sData.Length);
                    this.Command.Parameters[valueText].Value = sData;
                }
                return $"{f.Name} = {valueText}";
            }, ", ");
            return this;
        }

        private T Execute<T>(Func<T> func)
        {
            var res = func();
            return res;
        }

        private bool Serialize<T>(T obj, out byte[] binary)
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

        private bool Deserialize<T>(byte[] binary, out T obj)
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

        #region -- Commands --

        public IExecute MakeCommand(string command)
        {
            commandBuilder.Clear();
            commandBuilder.Append(command);
            return this;
        }

        #region -- Insert --

        public IInsert Insert(TTable data)
            => InsertCommandBuilder("INSERT", data, fieldInfos);

        public IInsert Insert(object data)
            => InsertCommandBuilder("INSERT", data, VerifyAndGetInfos(data));

        public IInsert InsertOrReplace(TTable data)
            => InsertCommandBuilder("REPLACE", data, fieldInfos);

        public IInsert InsertOrReplace(object data)
            => InsertCommandBuilder("REPLACE", data, VerifyAndGetInfos(data));

        #endregion

        #region -- Select --

        public ISelect<TTable> SelectAll()
        {
            commandBuilder.Append("SELECT * FROM ", TableName);
            return this;
        }

        public ISelect<TTable> Select(object fields)
        {
            var infos = VerifyAndGetInfos(fields);
            commandBuilder
                .Append("SELECT ")
                .Append(infos, i => i.Name, ", ")
                .Append(" FROM ", TableName);
            return this;
        }

        #endregion

        #region -- Update --

        public IUpdate<TTable> Update(TTable data)
            => UpdateCommandBuilder(data, fieldInfos);

        public IUpdate<TTable> Update(object data)
            => UpdateCommandBuilder(data, VerifyAndGetInfos(data));

        #endregion

        #region  -- Delete --

        public IDelete<TTable> Delete()
        {
            commandBuilder.Append("DELETE FROM ", TableName);
            return this;
        }

        #endregion

        #region -- Distinct --

        IDistinct<TTable> IDistinctable<TTable>.Distinct()
        {
            commandBuilder.Append(" DISTINCT");
            return this;
        }

        #endregion

        #region -- Where --

        IWhere<TTable> IWhereable<TTable, IWhere<TTable>>.Where(Expression<Func<bool>> expression)
        {
            commandBuilder.Append(" WHERE ", SqliteExpressionVisitor.GetSQL(expression));
            return this;
        }

        IWhere<TTable> IWhereable<TTable, IWhere<TTable>>.Where(Expression<Func<TTable, bool>> expression)
        {
            commandBuilder.Append(" WHERE ", SqliteExpressionVisitor.GetSQL(expression));
            return this;
        }

        IExecuteNonQuery IWhereable<TTable, IExecuteNonQuery>.Where(Expression<Func<bool>> expression)
        {
            commandBuilder.Append(" WHERE ", SqliteExpressionVisitor.GetSQL(expression));
            return this;
        }

        IExecuteNonQuery IWhereable<TTable, IExecuteNonQuery>.Where(Expression<Func<TTable, bool>> expression)
        {
            commandBuilder.Append(" WHERE ", SqliteExpressionVisitor.GetSQL(expression));
            return this;
        }

        #endregion

        #region -- GroupBy & Having --

        IGroupBy<TTable> IGroupByable<TTable>.GroupBy(object fields)
        {
            var infos = VerifyAndGetInfos(fields);
            commandBuilder
                .Append(" GROUP BY ")
                .Append(infos, i => i.Name, ", ");
            return this;
        }

        IHaving<TTable> IHavingable<TTable>.Having(Expression<Func<bool>> expression)
        {
            commandBuilder.Append(" HAVING ", SqliteExpressionVisitor.GetSQL(expression));
            return this;
        }

        IHaving<TTable> IHavingable<TTable>.Having(Expression<Func<TTable, bool>> expression)
        {
            commandBuilder.Append(" HAVING ", SqliteExpressionVisitor.GetSQL(expression));
            return this;
        }

        #endregion

        #region -- OrderBy --

        IOrderBy IOrderByable<TTable>.OrderBy(object fields, int sortBy)
        {
            var infos = VerifyAndGetInfos(fields);
            commandBuilder
                .Append(" ORDER BY ")
                .Append(infos, i => i.Name, ", ")
                .Append(sortBy < 0 ? " DESC" : " ASC");
            return this;
        }

        #endregion

        #region -- Limit --

        ILimit ILimitable.Limit(int count)
        {
            commandBuilder.Append(" LIMIT ", count);
            return this;
        }

        ILimit ILimitable.Limit(int count, int offset)
        {
            commandBuilder.Append(" LIMIT ", count, " OFFSET ", offset);
            return this;
        }

        #endregion

        #region -- Execute --

        int IExecuteNonQueryable.ExecuteNonQuery()
            => Execute(() => Command.ExecuteNonQuery(CommandText));


        SqliteDataReader IExecuteQueryable.ExecuteReader()
            => Execute(() => Command.ExecuteReader(CommandText));


        object IExecuteQueryable.ExecuteScalar()
            => Execute(() => Command.ExecuteScalar(CommandText));

        #region IDisposable Support
        private bool IsDisposed = false; // 偵測多餘的呼叫

        #endregion

        #endregion

        #endregion

        void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    try
                    {
                        DataReader?.Close();
                        Command?.Dispose();
                        Connection?.Dispose();
                    }
                    finally
                    {
                        DataReader = null;
                        Command = null;
                        Connection = null;
                    }
                }

                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

    }
}
