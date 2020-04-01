using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace Naukri.Sqlite
{
    /// <summary>
    /// 資料表工具
    /// </summary>
    /// <typeparam name="Table">目標資料表</typeparam>
    public sealed class NSqliteTable<Table>
        : IDisposable, IEnumerable<Table>, IEntry<Table>, IInsert, ISelect<Table>, IUpdate<Table>, IDelete<Table>, IDistinct<Table>
        , IWhere<Table>, IGroupBy<Table>, IHaving<Table>, IOrderBy<Table>, ILimit<Table>, IExecute, IExecuteQuery, IExecuteNonQuery
        where Table : new()
    {
        /// <summary>
        /// 資料庫連線工具
        /// </summary>
        public SqliteConnection Connection { get; set; }

        /// <summary>
        /// 資料庫指令工具
        /// </summary>
        public SqliteCommand Command { get; set; }

        /// <summary>
        /// 指令字串
        /// </summary>
        public string CommandText
        {
            get => commandBuilder.ToString();
            set => commandBuilder.Clear().Append(value);
        }

        private readonly StringBuilder commandBuilder = new StringBuilder();

        private readonly string tableName;

        private readonly NSqliteFieldInfo[] fieldInfos;

        private IEnumerable<NSqliteFieldInfo> NonAutoFieldInfos
        {
            get
            {
                foreach(var info in fieldInfos)
                {
                    if (info.Constraint.HasFlag(NSqliteConstraint.Autoincrement))
                    {
                        continue;
                    }
                    yield return info;
                }
            }
        }

        private bool IsDisposed = false;

        /// <summary>
        /// 建立資料表工具
        /// </summary>
        public NSqliteTable()
        {
            var info = NSqliteTableInfo.GetTableInfo<Table>();
            Connection = new SqliteConnection(info.ConnectionText);
            Command = new SqliteCommand(Connection);
            Connection.Open();
            tableName = info.Name;
            fieldInfos = info.FieldInfos;
        }

        private IEnumerable<NSqliteFieldInfo> VerifyAndGetInfos(object data, bool verifyAutoincrement = false)
        {
            // 取得資料架構
            var type = data.GetType();
            var infos = type.GetProperties(NSqlite.BINDING_FLAGS);
            // 驗證所有屬性皆具有 SqliteField 特性並回傳對應的 NSqliteFieldInfo[]
            for (int i = 0; i < infos.Length; i++)
            {
                bool isMatch = false;
                // 比對是否有相同的名稱被註冊 (是 NSqliteField)
                foreach (var field in fieldInfos)
                {
                    if (infos[i].Name == field.Info.Name)
                    {
                        if (verifyAutoincrement && field.Constraint.HasFlag(NSqliteConstraint.Autoincrement))
                        {
                            throw new Exception($"欄位\"{infos[i].Name}\" 含有 [Autoincrement] 屬性，故無法被設置");
                        }
                        yield return new NSqliteFieldInfo(field, infos[i]);
                        isMatch = true;
                        break;
                    }
                }
                if (!isMatch)
                {
                    throw new Exception($"欄位\"{infos[i].Name}\" 缺少 [SqliteField] 屬性，故無法被設置");
                }
            }
        }

        private IInsert InsertCommandBuilder(string command, object data, IEnumerable<NSqliteFieldInfo> fields)
        {
            commandBuilder
                .Clear()
                .Append(command, " INTO ", tableName, " (")
                .Append(fields, f => f.Name, ", ")
                .Append(") VALUES (")
                .Append(fields, f =>
                {
                    var valueText = f.GetValueText(data, out var blob);
                    if (NSqlite.Serialize(blob, out byte[] sData)) // 處理 BLOB 物件
                    {
                        Command.Prepare();
                        Command.Parameters.Add(valueText, DbType.Binary, sData.Length);
                        Command.Parameters[valueText].Value = sData;
                    }
                    return valueText;
                }, ", ")
                .Append(")");
            return this;
        }

        private IUpdate<Table> UpdateCommandBuilder(object data, IEnumerable<NSqliteFieldInfo> fields)
        {
            commandBuilder
                .Clear()
                .Append("UPDATE ", tableName, " SET ")
                .Append(fields, f =>
                {
                    var valueText = f.GetValueText(data, out var blob);
                    if (NSqlite.Serialize(blob, out byte[] sData)) // 處理 BLOB 物件
                    {
                        Command.Prepare();
                        Command.Parameters.Add(valueText, DbType.Binary, sData.Length);
                        Command.Parameters[valueText].Value = sData;
                    }
                    return $"{f.Name} = {valueText}";
                }, ", ");
            return this;
        }

        #region -- Commands --

        #region -- Insert --

        /// <summary>
        /// 建立一筆包含全部欄位的資料
        /// </summary>
        /// <param name="data">新增資料</param>
        public IInsert Insert(Table data)
            => InsertCommandBuilder("INSERT", data, NonAutoFieldInfos);

        /// <summary>
        /// 建立一筆包含特定欄位的資料
        /// </summary>
        /// <param name="data">新增資料</param>
        public IInsert Insert(object data)
            => InsertCommandBuilder("INSERT", data, VerifyAndGetInfos(data, true));

        /// <summary>
        /// 建立或取代一筆包含全部欄位的資料
        /// </summary>
        /// <param name="data">新增資料</param>
        public IInsert InsertOrReplace(Table data)
            => InsertCommandBuilder("REPLACE", data, NonAutoFieldInfos);

        /// <summary>
        /// 建立或取代一筆包含特定欄位的資料
        /// </summary>
        /// <param name="data">新增資料</param>
        public IInsert InsertOrReplace(object data)
            => InsertCommandBuilder("REPLACE", data, VerifyAndGetInfos(data, true));

        #endregion

        #region -- Select --

        /// <summary>
        /// 讀取全部欄位
        /// </summary>
        public ISelect<Table> SelectAll()
        {
            commandBuilder.Clear().Append("SELECT * FROM ", tableName);
            return this;
        }

        /// <summary>
        /// 讀取特定欄位
        /// </summary>
        /// <param name="fields">欄位</param>
        /// <returns></returns>
        public ISelect<Table> Select(object fields)
        {
            var infos = VerifyAndGetInfos(fields);
            commandBuilder
                .Clear()
                .Append("SELECT ")
                .Append(infos, i => i.Name, ", ")
                .Append(" FROM ", tableName);
            return this;
        }

        /// <summary>
        /// 讀取資料比數
        /// </summary>
        public ISelect<Table> Count()
        {
            commandBuilder
                .Clear()
                .Append("SELECT count(*) FROM ", tableName);
            return this;
        }

        #endregion

        #region -- Update --

        /// <summary>
        /// 更新一筆包含全部欄位的資料
        /// </summary>
        /// <param name="data">更新資料</param>
        public IUpdate<Table> Update(Table data)
            => UpdateCommandBuilder(data, NonAutoFieldInfos);

        /// <summary>
        /// 更新一筆包含特定欄位的資料
        /// </summary>
        /// <param name="data">更新資料</param>
        public IUpdate<Table> Update(object data)
            => UpdateCommandBuilder(data, VerifyAndGetInfos(data, true));

        #endregion

        #region  -- Delete --

        /// <summary>
        /// 刪除資料
        /// </summary>
        public IDelete<Table> Delete()
        {
            commandBuilder.Clear().Append("DELETE FROM ", tableName);
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
            => Command.ExecuteReader(CommandText);

        object IExecuteQueryable.ExecuteScalar()
            => Command.ExecuteScalar(CommandText);


        int IExecuteNonQueryable.ExecuteNonQuery()
            => Command.ExecuteNonQuery(CommandText);

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

        private class TableQuery<T> : IEnumerator<T> where T : new()
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
