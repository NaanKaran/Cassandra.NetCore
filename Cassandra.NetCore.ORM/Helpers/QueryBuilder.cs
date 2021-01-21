﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Cassandra.NetCore.ORM.Models;

namespace Cassandra.NetCore.ORM.Helpers
{
    static class QueryBuilder
    {
        public static Query EvaluateQuery<T>(Expression<Func<T, bool>> predicate)
        {
            var expression = predicate.Body as BinaryExpression;
            if (expression != null)
            {
                var q = EvaluateBinaryExpression(expression);
                return q;
            }

            return null;
        }

        public static string EvaluatePropertyName<T, TAverageModel>(Expression<Func<T, TAverageModel>> predicate)
        {
            var expression = predicate.Body as MemberExpression;
            return ExtractMemberNameFromExpression(expression);
        }

        private static object VisitMemberAccess(MemberExpression expression)
        {
            // there should only be two options. PropertyInfo or FieldInfo... let's extract the VALUE accordingly
            var value = new object();
            if ((expression.Member as PropertyInfo) != null)
            {
                var exp = (MemberExpression)expression.Expression;
                var constant = (ConstantExpression)exp.Expression;
                var fieldInfoValue = ((FieldInfo)exp.Member).GetValue(constant.Value);
                value = ((PropertyInfo)expression.Member).GetValue(fieldInfoValue, null);
            }
            else if ((expression.Member as FieldInfo) != null)
            {
                var fieldInfo = expression.Member as FieldInfo;
                var constantExpression = expression.Expression as ConstantExpression;
                if (fieldInfo != null & constantExpression != null)
                {
                    value = fieldInfo.GetValue(constantExpression.Value);
                }
            }

            return value;
        }

        public static object GetValue(MemberExpression memberExpression)
        {
            var constant = (ConstantExpression)memberExpression.Expression;
            var fieldInfoValue = ((FieldInfo)memberExpression.Member).GetValue(constant.Value);
            var value = ((PropertyInfo)memberExpression.Member).GetValue(fieldInfoValue, null);

            return value;
        }



        private static Query EvaluateBinaryExpression(BinaryExpression binaryExpression)
        {
            object value = null;
            string queryString = null, memberName = null;
            var operand = GetOperand(binaryExpression.NodeType);

            if (binaryExpression.Left is BinaryExpression expressionLeft && binaryExpression.Right is BinaryExpression expressionRight)
            {
                var left = EvaluateBinaryExpression(expressionLeft);
                var right = EvaluateBinaryExpression(expressionRight);
                queryString = $"{left.Statement} {operand} {right.Statement}";
                return new Query(queryString, left.Values.Concat(right.Values).ToArray());
            }
            else
            {
                var expressions = new[] { binaryExpression.Left, binaryExpression.Right };
                foreach (var exp in expressions)
                {
                    if (exp is MemberExpression)
                    {
                        var memberExpression = (MemberExpression)exp;
                        if (memberExpression.Expression.NodeType == ExpressionType.Parameter)
                        {
                            memberName = ExtractMemberNameFromExpression(memberExpression);
                        }
                        else
                        {
                            value = VisitMemberAccess(memberExpression);
                        }

                        continue;
                    }

                    if (exp.NodeType == ExpressionType.Convert || exp.NodeType == ExpressionType.Constant)
                    {
                        value = GetValue(binaryExpression.Right);
                        continue;
                    }
                }
            }

            queryString = $"{memberName} {operand} ?";

            return new Query(queryString, new[] { value });
        }

        private static string ExtractMemberNameFromExpression(MemberExpression memberExpression)
        {
            string memberName;

            var property = memberExpression.Member as PropertyInfo;
            if (property != null)
                memberName = property.GetColumnNameMapping();
            else
                memberName = memberExpression.Member.Name;

            return memberName;
        }

        private static string GetOperand(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return "and";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Not:
                    return "!";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.Or:
                    return "or";
                default:
                    throw new NotSupportedException($"Node type {nodeType} is a valid operand");
            }
        }

        private static object GetValue(Expression member)
        {
            var objectMember = Expression.Convert(member, typeof(object));

            var getterLambda = Expression.Lambda<Func<object>>(objectMember);

            var getter = getterLambda.Compile();

            return getter();
        }
    }
}