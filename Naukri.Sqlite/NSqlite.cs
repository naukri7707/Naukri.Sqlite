using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Naukri.Sqlite
{
    public static class NSqlite
    {
        internal const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static Action<string> Log { get; set; }

        public static NSqliteOption Option = NSqliteOption.CheckSchema;

        public static bool Serialize<T>(T obj, out byte[] binary)
        {
            if (obj == null)
            {
                binary = null;
                return false;
            }
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                binary = ms.ToArray();
                return true;
            }
        }

        public static bool Deserialize<T>(byte[] binary, out T obj)
        {
            if (binary == null)
            {
                obj = default;
                return false;
            }
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream(binary))
            {
                obj = (T)bf.Deserialize(ms);
                return true;
            }
        }

        public static void CreateTable<Table>(string connectionText)
        {
            NSqliteTableInfo.SetTableInfo<Table>(connectionText);
        }

        public static void CreateTable<Table1, Table2>(string connectionText)
        {
            NSqliteTableInfo.SetTableInfo<Table1>(connectionText);
            NSqliteTableInfo.SetTableInfo<Table2>(connectionText);
        }

        public static void CreateTable<Table1, Table2, Table3>(string connectionText)
        {
            NSqliteTableInfo.SetTableInfo<Table1>(connectionText);
            NSqliteTableInfo.SetTableInfo<Table2>(connectionText);
            NSqliteTableInfo.SetTableInfo<Table3>(connectionText);
        }

        public static void CreateTable<T1, T2, T3, T4>(string connectionText)
        {
            NSqliteTableInfo.SetTableInfo<T1>(connectionText);
            NSqliteTableInfo.SetTableInfo<T2>(connectionText);
            NSqliteTableInfo.SetTableInfo<T3>(connectionText);
            NSqliteTableInfo.SetTableInfo<T4>(connectionText);
        }

        public static void CreateTable<T1, T2, T3, T4, T5>(string connectionText)
        {
            NSqliteTableInfo.SetTableInfo<T1>(connectionText);
            NSqliteTableInfo.SetTableInfo<T2>(connectionText);
            NSqliteTableInfo.SetTableInfo<T3>(connectionText);
            NSqliteTableInfo.SetTableInfo<T4>(connectionText);
            NSqliteTableInfo.SetTableInfo<T5>(connectionText);
        }

        public static void CreateTable<T1, T2, T3, T4, T5, T6>(string connectionText)
        {
            NSqliteTableInfo.SetTableInfo<T1>(connectionText);
            NSqliteTableInfo.SetTableInfo<T2>(connectionText);
            NSqliteTableInfo.SetTableInfo<T3>(connectionText);
            NSqliteTableInfo.SetTableInfo<T4>(connectionText);
            NSqliteTableInfo.SetTableInfo<T5>(connectionText);
            NSqliteTableInfo.SetTableInfo<T6>(connectionText);
        }

        public static void CreateTable<T1, T2, T3, T4, T5, T6, T7>(string connectionText)
        {
            NSqliteTableInfo.SetTableInfo<T1>(connectionText);
            NSqliteTableInfo.SetTableInfo<T2>(connectionText);
            NSqliteTableInfo.SetTableInfo<T3>(connectionText);
            NSqliteTableInfo.SetTableInfo<T4>(connectionText);
            NSqliteTableInfo.SetTableInfo<T5>(connectionText);
            NSqliteTableInfo.SetTableInfo<T6>(connectionText);
            NSqliteTableInfo.SetTableInfo<T7>(connectionText);
        }

        public static void CreateTable<T1, T2, T3, T4, T5, T6, T7, T8>(string connectionText)
        {
            NSqliteTableInfo.SetTableInfo<T1>(connectionText);
            NSqliteTableInfo.SetTableInfo<T2>(connectionText);
            NSqliteTableInfo.SetTableInfo<T3>(connectionText);
            NSqliteTableInfo.SetTableInfo<T4>(connectionText);
            NSqliteTableInfo.SetTableInfo<T5>(connectionText);
            NSqliteTableInfo.SetTableInfo<T6>(connectionText);
            NSqliteTableInfo.SetTableInfo<T7>(connectionText);
            NSqliteTableInfo.SetTableInfo<T8>(connectionText);
        }
    }
}
