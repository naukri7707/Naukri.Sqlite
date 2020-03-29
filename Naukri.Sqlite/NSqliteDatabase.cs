using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
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
            if (tables.TryGetValue(type, out var nSqliteTable))
            {
                nSqliteTable = new NSqliteTable<T>(ConnectionText);
                tables[type] = nSqliteTable;
            }
            return nSqliteTable as NSqliteTable<T>;
        }
    }

}
