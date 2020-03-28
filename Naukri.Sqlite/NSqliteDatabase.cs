using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Naukri.Sqlite
{
    public class NSqliteDatabase
    {
        public static Dictionary<string, NSqliteDatabase> Find { get; } = new Dictionary<string, NSqliteDatabase>();
        
        public string Path { get; set; }

        public NSqliteDatabase(string path)
        {
            Path = path;
        }

        public NSqliteDatabase(string name, string path) : this(path)
        {
            Find[name] = this;
        }

        public SqliteConnection NewConnection() => new SqliteConnection(Path);

        public void NewConnection(Action<SqliteConnection> action)
        {
            try
            {
                var conn = new SqliteConnection(Path);
                conn.Open();
                action(conn);
                conn.Close();
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public async void NewConnectionAsync(Func<SqliteConnection, Task> action)
        {
            var conn = new SqliteConnection(Path);
            conn.Open();
            await action(conn);
            conn.Close();
        }

    }
}
