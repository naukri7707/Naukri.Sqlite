using System;
using System.Collections.Generic;
using System.Reflection;

namespace Naukri.Sqlite
{

    public struct NSqliteField
    {
        private static readonly Dictionary<Type, NSqliteDataType> sqliteType = new Dictionary<Type, NSqliteDataType>
        {
            // INTEGER
            {typeof(sbyte), NSqliteDataType.INTEGER },
            {typeof(byte), NSqliteDataType.INTEGER },
            {typeof(short), NSqliteDataType.INTEGER },
            {typeof(ushort), NSqliteDataType.INTEGER },
            {typeof(int), NSqliteDataType.INTEGER },
            {typeof(uint), NSqliteDataType.INTEGER },
            {typeof(long), NSqliteDataType.INTEGER },
            {typeof(ulong), NSqliteDataType.INTEGER },
            // REAL
            {typeof(float), NSqliteDataType.REAL },
            {typeof(double), NSqliteDataType.REAL },
            // NUMERIC
            {typeof(bool), NSqliteDataType.NUMERIC },
            {typeof(decimal), NSqliteDataType.NUMERIC },
            {typeof(DateTime), NSqliteDataType.NUMERIC },
            // TEXT
            {typeof(char), NSqliteDataType.TEXT },
            {typeof(string), NSqliteDataType.TEXT },
            // BLOB
            {typeof(object), NSqliteDataType.BLOB }
        };

        private readonly PropertyInfo info;

        public string Name { get; }

        public NSqliteDataType Type { get; }

        internal NSqliteField(PropertyInfo info)
        {
            this.info = info;
            Name = info.GetCustomAttribute<SqliteFieldAttribute>(true).Name ?? info.Name;
            Type = sqliteType.TryGetValue(info.PropertyType, out var type) ? type : NSqliteDataType.BLOB;
        }

        internal string GetValueText(object row, out object blob)
        {
            blob = null;
            var value = info.GetValue(row);
            if (value is null)
            {
                return "NULL";
            }
            switch (Type)
            {
                case NSqliteDataType.INTEGER:
                case NSqliteDataType.REAL:
                case NSqliteDataType.NUMERIC:
                    return value.ToString();
                case NSqliteDataType.TEXT:
                    return $"'{value}'";
                case NSqliteDataType.BLOB:
                    blob = value;
                    return $"@{Name}";
                default:
                    return "NULL";
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
