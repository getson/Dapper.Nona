//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Runtime.CompilerServices;
//using System.Text;

//namespace Dapper.Nona.IntegrationTests
//{
//    public class WhereBuilder
//    {
//        public string ToSql<T>(Expression<Func<T, bool>> expression)
//        {
//            return Recurse(expression.Body, true);
//        }

//        private string Recurse(Expression expression, bool isUnary = false, bool quote = true)
//        {
//            if (expression is UnaryExpression)
//            {
//                var unary = (UnaryExpression)expression;
//                var right = Recurse(unary.Operand, true);
//                return "(" + NodeTypeToString(unary.NodeType, right == "NULL") + " " + right + ")";
//            }
//            if (expression is BinaryExpression)
//            {
//                var body = (BinaryExpression)expression;
//                var right = Recurse(body.Right);
//                return "(" + Recurse(body.Left) + " " + NodeTypeToString(body.NodeType, right == "NULL") + " " + right + ")";
//            }
//            if (expression is ConstantExpression)
//            {
//                var constant = (ConstantExpression)expression;
//                return ValueToString(constant.Value, isUnary, quote);
//            }
//            if (expression is MemberExpression)
//            {
//                var member = (MemberExpression)expression;

//                if (member.Member is PropertyInfo)
//                {
//                    var property = (PropertyInfo)member.Member;
//                    var colName = _tableDef.GetColumnNameFor(property.Name);
//                    if (isUnary && member.Type == typeof(bool))
//                    {
//                        return "([" + colName + "] = 1)";
//                    }
//                    return "[" + colName + "]";
//                }
//                if (member.Member is FieldInfo)
//                {
//                    return ValueToString(GetValue(member), isUnary, quote);
//                }
//                throw new Exception($"Expression does not refer to a property or field: {expression}");
//            }
//            if (expression is MethodCallExpression)
//            {
//                var methodCall = (MethodCallExpression)expression;
//                // LIKE queries:
//                if (methodCall.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
//                {
//                    return "(" + Recurse(methodCall.Object) + " LIKE '%" + Recurse(methodCall.Arguments[0], quote: false) + "%')";
//                }
//                if (methodCall.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
//                {
//                    return "(" + Recurse(methodCall.Object) + " LIKE '" + Recurse(methodCall.Arguments[0], quote: false) + "%')";
//                }
//                if (methodCall.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
//                {
//                    return "(" + Recurse(methodCall.Object) + " LIKE '%" + Recurse(methodCall.Arguments[0], quote: false) + "')";
//                }
//                // IN queries:
//                if (methodCall.Method.Name == "Contains")
//                {
//                    Expression collection;
//                    Expression property;
//                    if (methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 2)
//                    {
//                        collection = methodCall.Arguments[0];
//                        property = methodCall.Arguments[1];
//                    }
//                    else if (!methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 1)
//                    {
//                        collection = methodCall.Object;
//                        property = methodCall.Arguments[0];
//                    }
//                    else
//                    {
//                        throw new Exception("Unsupported method call: " + methodCall.Method.Name);
//                    }
//                    var values = (IEnumerable)GetValue(collection);
//                    var concated = "";
//                    foreach (var e in values)
//                    {
//                        concated += ValueToString(e, false, true) + ", ";
//                    }
//                    if (concated == "")
//                    {
//                        return ValueToString(false, true, false);
//                    }
//                    return "(" + Recurse(property) + " IN (" + concated.Substring(0, concated.Length - 2) + "))";
//                }
//                throw new Exception("Unsupported method call: " + methodCall.Method.Name);
//            }
//            throw new Exception("Unsupported expression: " + expression.GetType().Name);
//        }

//        public string ValueToString(object value, bool isUnary, bool quote)
//        {
//            if (value is bool)
//            {
//                if (isUnary)
//                {
//                    return (bool)value ? "(1=1)" : "(1=0)";
//                }
//                return (bool)value ? "1" : "0";
//            }
//            return "";
//           // return _provider.ValueToString(value, quote);
//        }

//        private static bool IsEnumerableType(Type type)
//        {
//            return type.GetInterfaces()
//                       .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
//        }

//        private static object GetValue(Expression member)
//        {
//            // source: http://stackoverflow.com/a/2616980/291955
//            var objectMember = Expression.Convert(member, typeof(object));
//            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
//            var getter = getterLambda.Compile();
//            return getter();
//        }

//        private static object NodeTypeToString(ExpressionType nodeType, bool rightIsNull)
//        {
//            switch (nodeType)
//            {
//                case ExpressionType.Add:
//                    return "+";
//                case ExpressionType.And:
//                    return "&";
//                case ExpressionType.AndAlso:
//                    return "AND";
//                case ExpressionType.Divide:
//                    return "/";
//                case ExpressionType.Equal:
//                    return rightIsNull ? "IS" : "=";
//                case ExpressionType.ExclusiveOr:
//                    return "^";
//                case ExpressionType.GreaterThan:
//                    return ">";
//                case ExpressionType.GreaterThanOrEqual:
//                    return ">=";
//                case ExpressionType.LessThan:
//                    return "<";
//                case ExpressionType.LessThanOrEqual:
//                    return "<=";
//                case ExpressionType.Modulo:
//                    return "%";
//                case ExpressionType.Multiply:
//                    return "*";
//                case ExpressionType.Negate:
//                    return "-";
//                case ExpressionType.Not:
//                    return "NOT";
//                case ExpressionType.NotEqual:
//                    return "<>";
//                case ExpressionType.Or:
//                    return "|";
//                case ExpressionType.OrElse:
//                    return "OR";
//                case ExpressionType.Subtract:
//                    return "-";
//            }
//            throw new Exception($"Unsupported node type: {nodeType}");
//        }
//    }
//}
