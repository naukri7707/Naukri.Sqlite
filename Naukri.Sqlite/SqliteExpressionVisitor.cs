using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Naukri.Sqlite
{
    public class SqliteExpressionVisitor : ExpressionVisitor
    {
        public StringBuilder Query { get; private set; } = new StringBuilder();

        public static string GetSQL(Expression expression)
        {
            var v = new SqliteExpressionVisitor();
            v.Visit(expression);
            return v.Query.ToString();
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

        private string ConvertOperator(ExpressionType type)
        {
            switch (type)
            {
                // 算術運算符
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Modulo:
                    return "%";
                // 比較運算符
                case ExpressionType.Equal:
                    return "==";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                // 邏輯運算符
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.AndAlso:
                    return "AND";
                // 例外
                default:
                    throw new Exception("不合法的 SQL");
            }
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Visit(node.Left);
            Query.Append(ConvertOperator(node.NodeType));
            Visit(node.Right);
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Query.Append(node.Member.Name);
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            Query.Append(GetValueText(node.Value));
            return node;
        }
    }
}
