using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Naukri.Sqlite
{
    public abstract class NSqliteTable
    {
        public abstract string ConnectionText { get; }

        public abstract string TableName { get; }

        internal abstract NSqliteFieldInfo[] SqliteFields { get; }
    }

    public class NSqliteTable<TTable> : NSqliteTable, IEntry<TTable>
    {
        protected const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Dictionary<Type, (string, NSqliteFieldInfo[])> tableInfos = new Dictionary<Type, (string, NSqliteFieldInfo[])>();

        public static (string, NSqliteFieldInfo[]) GetTableInfo()
        {
            var type = typeof(TTable);
            if (!tableInfos.TryGetValue(type, out var tableInfo))
            {
                var table = typeof(TTable);
                var tableAttr = table.GetCustomAttribute<SqliteTableAttribute>();
                if (tableAttr is null)
                {
                    throw new Exception($"{typeof(TTable).Name} 需要有 Table 標籤 e.g. [SqliteTable(\"{typeof(TTable).Name}\")]");
                }
                // 取得有效的欄位資訊
                var props = table.GetProperties(BINDING_FLAGS);
                var fieldInfos = new NSqliteFieldInfo[props.Length];
                int len = 0;
                foreach (var prop in props)
                {
                    if (prop.GetCustomAttribute<SqliteFieldAttribute>() != null)
                    {
                        fieldInfos[len++] = new NSqliteFieldInfo(prop);
                    }
                }
                Array.Resize(ref fieldInfos, len);
                tableInfo = (tableAttr.Name, fieldInfos);
                tableInfos[type] = tableInfo;
            }
            return tableInfo;
        }

        public override string ConnectionText { get; }

        public override string TableName { get; }

        internal override NSqliteFieldInfo[] SqliteFields { get; }

        internal NSqliteTable(string connectionText)
        {
            // 取得連結字串以創建新的連線物件。
            ConnectionText = connectionText;
            // 取得資料表架構
            (TableName, SqliteFields) = GetTableInfo();
        }

        public NSqliteCommand<TTable> CreateCommand()
        {
            var conn = new SqliteConnection(ConnectionText);
            return new NSqliteCommand<TTable>(conn);
        }

        public IInsert Insert(TTable data)
            => CreateCommand().Insert(data);

        public IInsert Insert(object data)
            => CreateCommand().Insert(data);

        public IInsert InsertOrReplace(TTable data)
            => CreateCommand().InsertOrReplace(data);

        public IInsert InsertOrReplace(object data)
            => CreateCommand().InsertOrReplace(data);

        public ISelect<TTable> SelectAll()
            => CreateCommand().SelectAll();

        public ISelect<TTable> Select(object fields)
            => CreateCommand().Select(fields);

        public IUpdate<TTable> Update(TTable data)
            => CreateCommand().Update(data);

        public IUpdate<TTable> Update(object data)
            => CreateCommand().Update(data);

        public IDelete<TTable> Delete()
            => CreateCommand().Delete();
    }
}
