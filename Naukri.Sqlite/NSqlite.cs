using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Naukri.Sqlite
{
    public static class NSqlite
    {
        internal const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static Action<string> Log { get; set; }

        public static NSqliteOption Option = NSqliteOption.CheckSchema;

        public static Dictionary<string, NSqliteDatabase> Database { get; } = new Dictionary<string, NSqliteDatabase>();
    }
}
