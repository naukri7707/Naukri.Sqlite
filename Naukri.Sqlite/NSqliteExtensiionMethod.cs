using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Naukri.Sqlite
{
    public static class NSqliteExtensiionMethod
    {
        public static NSqliteCommand<T> CreateCommand<T>(this SqliteConnection self) where T : new()
        {
            return new NSqliteCommand<T>(self);
        }

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

        internal static StringBuilder Append<T>(this StringBuilder self, T[] values, string separate)
        {
            foreach (var value in values)
            {
                self.Append(value);
                self.Append(separate);
            }
            self.Length -= separate.Length;
            return self;
        }

        internal static StringBuilder Append<TArray, TValue>(this StringBuilder self, TArray[] values, Func<TArray, TValue> func, string separate)
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
