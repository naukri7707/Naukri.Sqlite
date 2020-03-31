using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Naukri.Sqlite
{
    internal static class NSqliteExtensiionMethod
    {
        internal static int ExecuteNonQuery(this SqliteCommand self, string command)
        {
            NSqlite.Log(command);
            self.CommandText = command;
            return self.ExecuteNonQuery();
        }

        internal static SqliteDataReader ExecuteReader(this SqliteCommand self, string command)
        {
            NSqlite.Log(command);
            self.CommandText = command;
            return self.ExecuteReader();
        }

        internal static object ExecuteScalar(this SqliteCommand self, string command)
        {
            NSqlite.Log(command);
            self.CommandText = command;
            return self.ExecuteScalar();
        }

        internal static StringBuilder Append(this StringBuilder self, params string[] values)
        {
            foreach (var value in values)
            {
                self.Append(value);
            }
            return self;
        }

        internal static StringBuilder Append(this StringBuilder self, params object[] values)
        {
            foreach (var value in values)
            {
                self.Append(value);
            }
            return self;
        }

        internal static StringBuilder Append<T>(this StringBuilder self, IEnumerable<T> values, string separate)
        {
            foreach (var value in values)
            {
                self.Append(value);
                self.Append(separate);
            }
            self.Length -= separate.Length;
            return self;
        }

        internal static StringBuilder Append<T, U>(this StringBuilder self, IEnumerable<T> values, Func<T, U> func, string separate)
        {
            foreach (var value in values)
            {
                self.Append(func(value));
                self.Append(separate);
            }
            self.Length -= separate.Length;
            return self;
        }
    }
}
