using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Naukri.Sqlite
{
    public sealed class NSqliteTable<Table>
        : IDisposable, IEnumerable<Table>, IEntry<Table>, IInsert, ISelect<Table>, IUpdate<Table>, IDelete<Table>, IDistinct<Table>
        , IWhere<Table>, IGroupBy<Table>, IHaving<Table>, IOrderBy<Table>, ILimit<Table>, IExecute, IExecuteQuery, IExecuteNonQuery
        where Table : new()
    {
        SqliteConnection Connection { get; set; }

        SqliteCommand Command { get; set; }

        internal NSqliteTableInfo Info { get; }

        public string TableName { get; }

        private readonly NSqliteFieldInfo[] fieldInfos;

        private readonly StringBuilder commandBuilder = new StringBuilder();

        public string CommandText => commandBuilder.ToString();

        private bool IsDisposed = false;

        public NSqliteTable()
        {
            var info = NSqliteTableInfo.GetTableInfo<Table>();
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
            commandBuilder.Clear();
            commandBuilder
            .Append(command, " INTO ", TableName, " (")
            .Append(fields, f => f.Name, ", ")
            .Append(") VALUES (")
            .Append(fields, (Func<NSqliteFieldInfo, string>)(f =>
            {
                var valueText = f.GetValueText(data, out var blob);
                if (NSqlite.Serialize(blob, out byte[] sData)) // 處理 BLOB 物件
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

        private IUpdate<Table> UpdateCommandBuilder(object data, NSqliteFieldInfo[] fields)
        {
            commandBuilder.Clear();
            commandBuilder
            .Append("UPDATE ", TableName, " SET ")
            .Append(fields, f =>
            {
                var valueText = f.GetValueText(data, out var blob);
                if (NSqlite.Serialize(blob, out byte[] sData)) // 處理 BLOB 物件
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
            commandBuilder.Clear();
            return res;
        }

        #region -- Commands --

        public IExecute MakeCommand(string command)
        {
            commandBuilder.Clear();
            commandBuilder.Append(command);
            return this;
        }

        #region -- Insert --

        public IInsert Insert(Table data)
            => InsertCommandBuilder("INSERT", data, fieldInfos);

        public IInsert Insert(object data)
            => InsertCommandBuilder("INSERT", data, VerifyAndGetInfos(data));

        public IInsert InsertOrReplace(Table data)
            => InsertCommandBuilder("REPLACE", data, fieldInfos);

        public IInsert InsertOrReplace(object data)
            => InsertCommandBuilder("REPLACE", data, VerifyAndGetInfos(data));

        #endregion

        #region -- Select --

        public ISelect<Table> SelectAll()
        {
            commandBuilder.Clear();
            commandBuilder.Append("SELECT * FROM ", TableName);
            return this;
        }

        public ISelect<Table> Select(object fields)
        {
            commandBuilder.Clear();
            var infos = VerifyAndGetInfos(fields);
            commandBuilder
                .Append("SELECT ")
                .Append(infos, i => i.Name, ", ")
                .Append(" FROM ", TableName);
            return this;
        }

        #endregion

        #region -- Update --

        public IUpdate<Table> Update(Table data)
            => UpdateCommandBuilder(data, fieldInfos);

        public IUpdate<Table> Update(object data)
            => UpdateCommandBuilder(data, VerifyAndGetInfos(data));

        #endregion

        #region  -- Delete --

        public IDelete<Table> Delete()
        {
            commandBuilder.Clear();
            commandBuilder.Append("DELETE FROM ", TableName);
            return this;
        }

        #endregion

        #region -- Distinct --

        IDistinct<Table> IDistinctable<Table>.Distinct()
        {
            commandBuilder.Append(" DISTINCT");
            return this;
        }

        #endregion

        #region -- Where --

        IWhere<Table> IWhereable<Table, IWhere<Table>>.Where(Expression<Func<bool>> expression)
        {
            commandBuilder.Append(" WHERE ", SqliteExpressionVisitor.GetSQL(expression));
            return this;
        }

        IWhere<Table> IWhereable<Table, IWhere<Table>>.Where(Expression<Func<Table, bool>> expression)
        {
            commandBuilder.Append(" WHERE ", SqliteExpressionVisitor.GetSQL(expression));
            return this;
        }

        IExecuteNonQuery IWhereable<Table, IExecuteNonQuery>.Where(Expression<Func<bool>> expression)
        {
            commandBuilder.Append(" WHERE ", SqliteExpressionVisitor.GetSQL(expression));
            return this;
        }

        IExecuteNonQuery IWhereable<Table, IExecuteNonQuery>.Where(Expression<Func<Table, bool>> expression)
        {
            commandBuilder.Append(" WHERE ", SqliteExpressionVisitor.GetSQL(expression));
            return this;
        }

        #endregion

        #region -- GroupBy & Having --

        IGroupBy<Table> IGroupByable<Table>.GroupBy(object fields)
        {
            var infos = VerifyAndGetInfos(fields);
            commandBuilder
                .Append(" GROUP BY ")
                .Append(infos, i => i.Name, ", ");
            return this;
        }

        IHaving<Table> IHavingable<Table>.Having(Expression<Func<bool>> expression)
        {
            commandBuilder.Append(" HAVING ", SqliteExpressionVisitor.GetSQL(expression));
            return this;
        }

        IHaving<Table> IHavingable<Table>.Having(Expression<Func<Table, bool>> expression)
        {
            commandBuilder.Append(" HAVING ", SqliteExpressionVisitor.GetSQL(expression));
            return this;
        }

        #endregion

        #region -- OrderBy --

        IOrderBy<Table> IOrderByable<Table>.OrderBy(object fields, int sortBy)
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

        ILimit<Table> ILimitable<Table>.Limit(int count)
        {
            commandBuilder.Append(" LIMIT ", count);
            return this;
        }

        ILimit<Table> ILimitable<Table>.Limit(int count, int offset)
        {
            commandBuilder.Append(" LIMIT ", count, " OFFSET ", offset);
            return this;
        }

        #endregion

        #region -- Execute --

        SqliteDataReader IExecuteQueryable.ExecuteReader()
            => Execute(() => Command.ExecuteReader(CommandText));

        object IExecuteQueryable.ExecuteScalar()
            => Execute(() => Command.ExecuteScalar(CommandText));


        int IExecuteNonQueryable.ExecuteNonQuery()
            => Execute(() => Command.ExecuteNonQuery(CommandText));

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
                        Command?.Dispose();
                        Connection?.Dispose();
                    }
                    finally
                    {
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

        public IEnumerator<Table> GetEnumerator()
        {
            var reader = Command.ExecuteReader(CommandText);
            return new TableQuery<Table>(reader, fieldInfos);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal class TableQuery<T> : IEnumerator<T> where T : new()
        {
            public T Current
            {
                get
                {
                    var res = new T();
                    for (int i = fields.Length - 1; i >= 0; i--)
                    {
                        var dbData = reader[i];
                        if (dbData is DBNull)
                        {
                            continue;
                        }
                        object data;
                        if (fields[i].Type == NSqliteDataType.BLOB)
                        {
                            if (!NSqlite.Deserialize(dbData as byte[], out data))
                            {
                                throw new Exception("Deserialize Fail");
                            }
                        }
                        else
                        {
                            data = Convert.ChangeType(dbData, fields[i].Info.PropertyType);
                        }
                        fields[i].Info.SetValue(res, data);
                    }
                    return res;
                }
            }

            T IEnumerator<T>.Current => Current;

            object IEnumerator.Current => Current;

            private readonly SqliteDataReader reader;

            private readonly NSqliteFieldInfo[] fields;

            internal TableQuery(SqliteDataReader reader, NSqliteFieldInfo[] fields)
            {
                this.reader = reader;
                this.fields = new NSqliteFieldInfo[reader.FieldCount];
                int len = 0;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    foreach (var field in fields)
                    {
                        if (field.Name == reader.GetName(i))
                        {
                            this.fields[len++] = field;
                        }
                    }
                }
                Array.Resize(ref this.fields, len);
            }

            public bool MoveNext()
            {
                return reader.Read();
            }

            public void Reset()
            {
                throw new NotImplementedException("不支援反覆讀取");
            }

            public void Dispose()
            {
                reader.Close();
            }
        }
    }
}
