using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Naukri.Sqlite
{
    public class NSqliteDatabase
    {
        public static Dictionary<string, NSqliteDatabase> Find { get; } = new Dictionary<string, NSqliteDatabase>();

        public string ConnectionText { get; }

        public NSqliteDatabase(string path)
        {
            ConnectionText = path;
        }

        public NSqliteDatabase(string name, string path) : this(path)
        {
            Find[name] = this;
        }

        public readonly Dictionary<Type, NSqliteTable> tables = new Dictionary<Type, NSqliteTable>();

        public NSqliteTable<T> Table<T>()
        {
            var type = typeof(T);
            if (!tables.TryGetValue(type, out var table))
            {
                // 產生 C# 端資料架構
                table = new NSqliteTable<T>(ConnectionText);
                tables[type] = table;

                // 產生 CreateTable 的 Query
                StringBuilder CreateSchema()
                {
                    var queryBuilder = new StringBuilder($"CREATE TABLE {table.TableName} (");
                    foreach (var field in table.SqliteFields)
                    {
                        var attributes = field.Info.GetCustomAttributes(typeof(SqliteConstraintAttribute), true) as SqliteConstraintAttribute[];
                        queryBuilder
                            .Append(field.Name, ' ', field.Type, ' ')   // 欄位資料
                            .Append(attributes, attr => attr.Text, " ") // 欄位約束
                            .Append(",");
                    }
                    queryBuilder.Length--;
                    queryBuilder.Append(")"); // 因為 Sqlite 回傳的沒有分號，所以不加
                    return queryBuilder;
                }

                // 連結至 Sqlite 新增或驗證資料架構
                var conn = new SqliteConnection(ConnectionText);
                conn.Open();
                var cmd = conn.CreateCommand();
                // 若 Sqlite 無同名的資料表，新增之
                if (cmd.ExecuteScalar($"SELECT name FROM sqlite_master WHERE type='table' AND name='{table.TableName}';") is null)
                {
                    var query = CreateSchema().Append(';').ToString();
                    cmd.ExecuteNonQuery(query);
                }
                // 若 Sqlite 有同名的資料表，驗證之
                else if (NSqlite.Option.HasFlag(NSqliteOption.CheckSchema))
                {
                    var sqliteSchema = cmd.ExecuteScalar($"SELECT sql FROM sqlite_master WHERE name == '{table.TableName}'").ToString();
                    if (sqliteSchema != CreateSchema().ToString())
                    {
                        throw new Exception("C# 與 Sqlite 端的資料表架構不相同");
                    }
                }
                cmd.Dispose();
                conn.Close();
            }
            return table as NSqliteTable<T>;
        }

    }
}
