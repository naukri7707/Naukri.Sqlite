using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Naukri.Sqlite
{
    internal class SqliteExpressionVisitor : ExpressionVisitor
    {
        private readonly StringBuilder query  = new StringBuilder();

        internal static string GetSQL(Expression expression)
        {
            var v = new SqliteExpressionVisitor();
            v.Visit(expression);
            return v.query.ToString();
        }

        private string GetValueText(object obj)
        {
            switch (obj)
            {
                case string _:
                case char _:
                    return $"'{obj}'";
                default:
                    return obj.ToString();
            }
        }

        private string ConvertOperator(ExpressionType type, out bool brackets)
        {
            brackets = true;
            switch (type)
            {
                // 算術運算子
                case ExpressionType.Add:
                    return " + ";
                case ExpressionType.Subtract:
                    return " - ";
                case ExpressionType.Multiply:
                    return " * ";
                case ExpressionType.Divide:
                    return " / ";
                case ExpressionType.Modulo:
                    return " % ";
                // 比較運算子
                case ExpressionType.Equal:
                    brackets = false;
                    return " == ";
                case ExpressionType.NotEqual:
                    brackets = false;
                    return " != ";
                case ExpressionType.GreaterThan:
                    brackets = false;
                    return " > ";
                case ExpressionType.GreaterThanOrEqual:
                    brackets = false;
                    return " >= ";
                case ExpressionType.LessThan:
                    brackets = false;
                    return " < ";
                case ExpressionType.LessThanOrEqual:
                    brackets = false;
                    return " <= ";
                // 邏輯運算子
                case ExpressionType.OrElse:
                    return " OR ";
                case ExpressionType.AndAlso:
                    return " AND ";
                // 位元運算子
                case ExpressionType.And:
                    return " & ";
                case ExpressionType.Or:
                    return " | ";
                case ExpressionType.OnesComplement:
                    return "~";
                case ExpressionType.LeftShift:
                    return "<<";
                case ExpressionType.RightShift:
                    return ">>";
                // 例外
                default:
                    throw new Exception("不合法的 SQL");
            }
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var sb = ConvertOperator(node.NodeType, out bool brackets);
            if (brackets) query.Append('(');
            Visit(node.Left);
            query.Append(sb);
            Visit(node.Right);
            if (brackets) query.Append(')');
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.GetCustomAttribute<SqliteFieldAttribute>() is null)
            {
                var member = Expression.Convert(node, typeof(object));
                var lambda = Expression.Lambda<Func<object>>(member);
                var value = lambda.Compile();
                query.Append(value());
            }
            else
            {
                query.Append(node.Member.Name);
            }
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            query.Append(GetValueText(node.Value));
            return node;
        }
    }
}
