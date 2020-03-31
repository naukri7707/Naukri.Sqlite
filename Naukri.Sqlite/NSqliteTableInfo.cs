using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Naukri.Sqlite
{
    internal class NSqliteTableInfo
    {
        internal static readonly Dictionary<Type, NSqliteTableInfo> tableInfos = new Dictionary<Type, NSqliteTableInfo>();

        internal static NSqliteTableInfo GetTableInfo<T>()
        {
            return tableInfos.TryGetValue(typeof(T), out var info) ? info : null;
        }

        internal static void SetTableInfo<T>(string connectionText)
        {
            var type = typeof(T);
            var tableAttr = type.GetCustomAttribute<SqliteTableAttribute>();
            if (tableAttr is null)
            {
                throw new Exception($"{type.Name} 需要有 Table 標籤 e.g. [SqliteTable(\"{type.Name}\")]");
            }
            // 取得有效的欄位資訊
            var props = type.GetProperties(NSqlite.BINDING_FLAGS);
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
            // TODO 資料庫端驗證
            using (var conn = new SqliteConnection(connectionText))
            {
                StringBuilder CreateSchema()
                {
                    var queryBuilder = new StringBuilder($"CREATE TABLE {tableAttr.Name} (");
                    foreach (var field in fieldInfos)
                    {
                        var attributes = field.Info.GetCustomAttributes<SqliteConstraintAttribute>(true);
                        queryBuilder
                            .Append(field.Name, ' ', field.Type, ' ')   // 欄位資料
                            .Append(attributes, attr => attr.Text, " ") // 欄位約束
                            .Append(",");
                    }
                    queryBuilder.Length--;
                    queryBuilder.Append(")"); // 因為 Sqlite 回傳的沒有分號，所以不加
                    return queryBuilder;
                }

                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    // 若 Sqlite 無同名的資料表，新增之
                    if (cmd.ExecuteScalar($"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableAttr.Name}';") is null)
                    {
                        var query = CreateSchema().Append(';').ToString();
                        cmd.ExecuteNonQuery(query);
                    }
                    // 若 Sqlite 有同名的資料表，驗證之
                    else if (NSqlite.Option.HasFlag(NSqliteOption.CheckSchema))
                    {
                        var sqliteSchema = cmd.ExecuteScalar($"SELECT sql FROM sqlite_master WHERE name == '{tableAttr.Name}'").ToString();
                        if (sqliteSchema != CreateSchema().ToString())
                        {
                            throw new Exception("C# 與 Sqlite 端的資料表架構不相同");
                        }
                    }
                }
            }


            tableInfos[type] = new NSqliteTableInfo(tableAttr.Name, fieldInfos, connectionText);
        }

        private NSqliteTableInfo(string name, NSqliteFieldInfo[] infos, string connectionText)
        {
            Name = name;
            FieldInfos = infos;
            ConnectionText = connectionText;
        }

        internal string Name { get; }

        internal string ConnectionText { get; }

        internal NSqliteFieldInfo[] FieldInfos { get; }
    }
}
