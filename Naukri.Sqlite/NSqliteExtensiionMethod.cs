using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Naukri.Sqlite
{
    public static class NSqliteExtensiionMethod
    {
        public static NSqliteCommand<T> CreateCommand<T>(this SqliteConnection self) where T : new()
        {
            return new NSqliteCommand<T>(self);
        }

        internal static StringBuilder Append(this StringBuilder self, params string[] values)
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
