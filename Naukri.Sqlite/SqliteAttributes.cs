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

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class SqliteConstraintAttribute : SqliteAttribute
    {
        public abstract NSqliteConstraint Constraint { get; }
    }

    public sealed class PrimaryKeyAttribute : SqliteConstraintAttribute
    {
        public override NSqliteConstraint Constraint => NSqliteConstraint.PrimaryKey;

        public override string ToString() => "PRIMARY KEY";
    }

    public sealed class UniqueAttribute : SqliteConstraintAttribute
    {
        public override NSqliteConstraint Constraint => NSqliteConstraint.Unique;

        public override string ToString() => "UNIQUE";
    }

    public sealed class NotNullAttribute : SqliteConstraintAttribute
    {
        public override NSqliteConstraint Constraint => NSqliteConstraint.NotNull;

        public override string ToString() => "NOT NULL";
    }

    public sealed class DefaultAttribute : SqliteConstraintAttribute
    {
        public override NSqliteConstraint Constraint => NSqliteConstraint.Default;

        private readonly string text;

        public DefaultAttribute(object defaultValue)
        {
            text = $"DEFAULT {defaultValue}";
        }

        public override string ToString() => text;
    }

    public sealed class CheckAttribute : SqliteConstraintAttribute
    {
        public override NSqliteConstraint Constraint => NSqliteConstraint.Check;
       
        private readonly string text;

        public CheckAttribute(string checkExpression)
        {
            text = $"CHECK({checkExpression})";
        }

        public override string ToString() => text;
    }

    public sealed class AutoincrementAttribute : SqliteConstraintAttribute
    {
        public override NSqliteConstraint Constraint => NSqliteConstraint.Autoincrement;

        public override string ToString() => "AUTOINCREMENT";
    }
}
