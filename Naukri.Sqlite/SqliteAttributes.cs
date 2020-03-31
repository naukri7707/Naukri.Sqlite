using System;

namespace Naukri.Sqlite
{
    public abstract class SqliteAttribute : Attribute
    {
        
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class SqliteTableAttribute : SqliteAttribute
    {
        public string Name { get; }

        public SqliteTableAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SqliteFieldAttribute : SqliteAttribute
    {
        public string Name { get; }

        public SqliteFieldAttribute()
        {
        }

        public SqliteFieldAttribute(string fieldName)
        {
            Name = fieldName;
        }
    }

    public abstract class SqliteConstraintAttribute : SqliteAttribute
    {
        public abstract string Text { get; }

        public override string ToString()
        {
            return Text;
        }
    }

    public sealed class PrimaryKeyAttribute : SqliteConstraintAttribute
    {
        public override string Text => "PRIMARY KEY";
    }

    public sealed class UniqueAttribute : SqliteConstraintAttribute
    {
        public override string Text => "UNIQUE";
    }

    public sealed class NotNullAttribute : SqliteConstraintAttribute
    {
        public override string Text => "NOT NULL";
    }

    sealed class DefaultAttribute : SqliteConstraintAttribute
    {
        public override string Text => $"DEFAULT {DefaultValue}";

        public object DefaultValue { get; }

        public DefaultAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;
        }
    }

    public sealed class CheckAttribute : SqliteConstraintAttribute
    {
        public override string Text => $"CHECK({CheckValue})";

        public string CheckValue { get; }

        public CheckAttribute(string checkValue)
        {
            CheckValue = checkValue;
        }
    }

    public sealed class AutoincrementAttribute : SqliteConstraintAttribute
    {
        public override string Text => "AUTOINCREMENT";
    }
}
