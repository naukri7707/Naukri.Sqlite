using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;

namespace Naukri.Sqlite
{
    public static class NSqlite
    {
        internal const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// 查詢記錄
        /// </summary>
        public static Action<string> Log { get; set; }

        /// <summary>
        /// NSqlite 可選項
        /// </summary>
        public static NSqliteOption Option = NSqliteOption.CheckSchema;

        /// <summary>
        /// 序列化目標物件成 byte[]
        /// </summary>
        /// <typeparam name="T">物件型態</typeparam>
        /// <param name="obj">目標物件</param>
        /// <param name="binary">序列化物件</param>
        /// <returns>是否完成序列化</returns>
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

        /// <summary>
        /// 反序列化 byte[] 給目標物件
        /// </summary>
        /// <typeparam name="T">物件型態</typeparam>
        /// <param name="binary">序列化物件</param>
        /// <param name="obj">目標物件</param>
        /// <returns>是否完成反序列化</returns>
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

        /// <summary>
        /// 建立資料表
        /// </summary>
        /// <typeparam name="Table">目標資料表</typeparam>
        /// <param name="connectionText">資料庫連線</param>
        public static void CreateTable<Table>(string connectionText)
        {
            NSqliteTableInfo.CreateTableInfo<Table>(connectionText);
        }

        public static void CreateTable<Table1, Table2>(string connectionText)
        {
            NSqliteTableInfo.CreateTableInfo<Table1>(connectionText);
            NSqliteTableInfo.CreateTableInfo<Table2>(connectionText);
        }

        public static void CreateTable<Table1, Table2, Table3>(string connectionText)
        {
            NSqliteTableInfo.CreateTableInfo<Table1>(connectionText);
            NSqliteTableInfo.CreateTableInfo<Table2>(connectionText);
            NSqliteTableInfo.CreateTableInfo<Table3>(connectionText);
        }

        public static void CreateTable<T1, T2, T3, T4>(string connectionText)
        {
            NSqliteTableInfo.CreateTableInfo<T1>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T2>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T3>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T4>(connectionText);
        }

        public static void CreateTable<T1, T2, T3, T4, T5>(string connectionText)
        {
            NSqliteTableInfo.CreateTableInfo<T1>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T2>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T3>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T4>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T5>(connectionText);
        }

        public static void CreateTable<T1, T2, T3, T4, T5, T6>(string connectionText)
        {
            NSqliteTableInfo.CreateTableInfo<T1>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T2>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T3>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T4>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T5>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T6>(connectionText);
        }

        public static void CreateTable<T1, T2, T3, T4, T5, T6, T7>(string connectionText)
        {
            NSqliteTableInfo.CreateTableInfo<T1>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T2>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T3>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T4>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T5>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T6>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T7>(connectionText);
        }

        public static void CreateTable<T1, T2, T3, T4, T5, T6, T7, T8>(string connectionText)
        {
            NSqliteTableInfo.CreateTableInfo<T1>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T2>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T3>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T4>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T5>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T6>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T7>(connectionText);
            NSqliteTableInfo.CreateTableInfo<T8>(connectionText);
        }
    }
}
