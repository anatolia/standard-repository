using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using StandardRepository.Models;

namespace StandardRepository.Helpers
{
    public abstract class ExpressionUtils
    {
        private readonly string _prmPrefix;
        private readonly string _prmSign;
        private readonly string _prmPrefixForVariable;

        protected ExpressionUtils(string prmPrefix = null, string prmSign = null)
        {
            _prmPrefix = prmPrefix ?? SQLConstants.PARAMETER_PREFIX;
            _prmSign = prmSign ?? SQLConstants.PARAMETER_PRESIGN;
            _prmPrefixForVariable = _prmSign + "var" + _prmPrefix;
        }

        public abstract DbType GetDbType(Type type);

        public string GetConditions(Expression expression, Dictionary<string, DbParameterInfo> parameters)
        {
            if (expression is MemberExpression member)
            {
                if (member.Member.DeclaringType == typeof(DateTime)
                    || member.Member.DeclaringType == typeof(DateTime?))
                {
                    var objectMember = Expression.Convert(member, typeof(object));
                    var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                    var getter = getterLambda.Compile();
                    var value = getter();

                    var prmName = AddToParameters(parameters, "date", member.Member.DeclaringType, value);
                    return prmName;
                }

                if (member.Expression is MemberExpression)
                {
                    if (member.Member is PropertyInfo memberPropertyInfo)
                    {
                        var lambdaExpression = Expression.Lambda(expression);
                        var value = lambdaExpression.Compile().DynamicInvoke();
                        var prmName = member.Member.Name.GetFieldNameFromPropertyName(member.Expression.Type.Name);
                        prmName = AddToParameters(parameters, prmName, memberPropertyInfo.PropertyType, value);
                        return prmName;
                    }
                }
                var fieldName = member.Member.Name.GetFieldNameFromPropertyName(member.Expression.Type.Name);

                if (member.Member is FieldInfo memberFieldInfo)
                {
                    var lambdaExpression = Expression.Lambda(expression);
                    var value = lambdaExpression.Compile().DynamicInvoke();
                    var prmName = AddToParameters(parameters, fieldName, memberFieldInfo.FieldType, value);
                    return prmName;
                }

                return fieldName;
            }

            if (expression is UnaryExpression unary)
            {
                var nodeTypeToString = NodeTypeToString(unary.NodeType);
                if (string.IsNullOrWhiteSpace(nodeTypeToString))
                {
                    return GetConditions(unary.Operand, parameters);
                }

                return $"{nodeTypeToString} {GetConditions(unary.Operand, parameters)}";
            }

            if (expression is BinaryExpression body)
            {
                if (body.Left.NodeType == ExpressionType.Constant
                    && body.Right.NodeType == ExpressionType.Constant)
                {
                    throw new ArgumentException("unsupported expression!");
                }

                var formatPattern = "{0} {1} {2}";
                if (IsNodeNeedingParentheses(body.NodeType))
                {
                    formatPattern = "({0}) {1} ({2})";
                }

                if (body.Right.NodeType == ExpressionType.Constant)
                {
                    var value = ((ConstantExpression)body.Right).Value;
                    var memberExpression = (MemberExpression)body.Left;

                    var fieldName = memberExpression.Member.Name.GetFieldNameFromPropertyName(memberExpression.Expression.Type.Name);
                    var prmName = AddToParameters(parameters, fieldName, memberExpression.Type, value);

                    return string.Format(formatPattern, fieldName, NodeTypeToString(body.NodeType), prmName);
                }

                if (body.Left.NodeType == ExpressionType.Constant)
                {
                    var value = ((ConstantExpression)body.Left).Value;
                    var memberExpression = (MemberExpression)body.Right;
                    var fieldName = memberExpression.Member.Name.GetFieldNameFromPropertyName(memberExpression.Expression.Type.Name);
                    var prmName = AddToParameters(parameters, fieldName, memberExpression.Type, value);

                    return string.Format(formatPattern, prmName, NodeTypeToString(body.NodeType), fieldName);
                }

                string rightPart;
                try
                {
                    var memberExpressionRight = (MemberExpression)body.Right;
                    var fieldName = memberExpressionRight.Member.Name.GetFieldNameFromPropertyName(memberExpressionRight.Expression.Type.Name);
                    var lambdaExpressionRight = Expression.Lambda(memberExpressionRight);
                    var valueRight = lambdaExpressionRight.Compile().DynamicInvoke();
                    rightPart = AddToParameters(parameters, fieldName, memberExpressionRight.Type, valueRight);
                }
                catch (Exception)
                {
                    rightPart = GetConditions(body.Right, parameters);
                }

                string leftPart;
                try
                {
                    var memberExpressionLeft = (MemberExpression)body.Left;
                    var fieldName = memberExpressionLeft.Member.Name.GetFieldNameFromPropertyName(memberExpressionLeft.Expression.Type.Name);
                    var lambdaExpressionLeft = Expression.Lambda(memberExpressionLeft);
                    var valueLeft = lambdaExpressionLeft.Compile().DynamicInvoke();
                    leftPart = AddToParameters(parameters, fieldName, memberExpressionLeft.Type, valueLeft);
                }
                catch (Exception)
                {
                    leftPart = GetConditions(body.Left, parameters);
                }

                return string.Format(formatPattern, leftPart, NodeTypeToString(body.NodeType), rightPart);
            }

            if (expression is MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.Name == "Contains")
                {
                    var lambda = Expression.Lambda(methodCallExpression.Arguments[0]);
                    var compiled = lambda.Compile();
                    var value = compiled.DynamicInvoke();

                    if (methodCallExpression.Object is MemberExpression memberAccess)
                    {
                        var fieldName = memberAccess.Member.Name.GetFieldNameFromPropertyName(memberAccess.Expression.Type.Name);
                        var prmName = AddToParameters(parameters, fieldName, typeof(string), value);

                        return $"LOWER({fieldName}) LIKE '%' || {prmName} || '%'";
                    }
                }
            }

            if (expression is ConstantExpression constantExpression)
            {
                var value = constantExpression.Value;

                var prmName = AddToParameters(parameters, "constant", constantExpression.Type, value);
                return prmName;
            }

            throw new NotSupportedException("not supported expression > " + expression);
        }

        private string AddToParameters(Dictionary<string, DbParameterInfo> parameters, string fieldName, Type type, object value)
        {
            var prmType = GetDbType(type);
            var prmName = _prmPrefixForVariable + fieldName;
            if (parameters.ContainsKey(prmName))
            {
                prmName = prmName + "_" + (parameters.Keys.Count(x => x.StartsWith(prmName)) + 1);
            }

            parameters.Add(prmName, new DbParameterInfo(prmName, value ?? DBNull.Value, prmType));
            return prmName;
        }

        public string GetFieldName(Expression expression)
        {
            if (expression is UnaryExpression unary)
            {
                return GetFieldName(unary.Operand);
            }

            if (expression is MemberExpression member)
            {
                return member.Member.Name.GetFieldNameFromPropertyName(member.Expression.Type.Name);
            }

            return string.Empty;
        }

        public Type GetFieldType(Expression expression)
        {
            if (expression is UnaryExpression unary)
            {
                return GetFieldType(unary.Operand);
            }

            if (expression is MemberExpression member)
            {
                return member.Expression.Type;
            }

            return null;
        }

        public string NodeTypeToString(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.And:
                    return "&";
                case ExpressionType.AndAlso:
                    return "AND";
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
                case ExpressionType.Not:
                    return "NOT";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return string.Empty;
                default:
                    throw new NotSupportedException("not supported nodeType for query generation > " + nodeType);
            }
        }

        public bool IsNodeNeedingParentheses(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Not:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return true;
                default:
                    return false;
            }
        }
    }
}