using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Alterity.SlimAPI
{
    public static class DelegateFactory
    {
        public static Func<Object, Object[], Object> CreateForNonVoidInstanceMethod(MethodInfo method)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(Object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(Object[]), "arguments");

            MethodCallExpression call = Expression.Call(
              Expression.Convert(instanceParameter, method.DeclaringType),
              method,
              CreateParameterExpressions(method, argumentsParameter));

            Expression<Func<Object, Object[], Object>> lambda = Expression.Lambda<Func<Object, Object[], Object>>(
              Expression.Convert(call, typeof(Object)),
              instanceParameter,
              argumentsParameter);

            return lambda.Compile();
        }

        private static Func<Object[], Object> CreateForNonVoidStaticMethod(MethodInfo method)
        {
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(Object[]), "arguments");

            MethodCallExpression call = Expression.Call(              
              method,
              CreateParameterExpressions(method, argumentsParameter));

            Expression<Func<Object[], Object>> lambda = Expression.Lambda<Func<Object[], Object>>(
              Expression.Convert(call, typeof(Object)),
              argumentsParameter);

            return lambda.Compile();
        }

        public static Action<Object, Object[]> CreateForVoidInstanceMethod(MethodInfo method)
        {
            ParameterExpression instanceParameter = Expression.Parameter(typeof(Object), "target");
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(Object[]), "arguments");

            MethodCallExpression call = Expression.Call(
              Expression.Convert(instanceParameter, method.DeclaringType),
              method,
              CreateParameterExpressions(method, argumentsParameter));

            Expression<Action<Object, Object[]>> lambda = Expression.Lambda<Action<Object, Object[]>>(
              call,
              instanceParameter,
              argumentsParameter);

            return lambda.Compile();
        }

        private static Action<Object[]> CreateForVoidStaticMethod(MethodInfo method)
        {
            ParameterExpression argumentsParameter = Expression.Parameter(typeof(Object[]), "arguments");

            MethodCallExpression call = Expression.Call(
              method,
              CreateParameterExpressions(method, argumentsParameter));

            Expression<Action<Object[]>> lambda = Expression.Lambda<Action<Object[]>>(
              call,
              argumentsParameter);

            return lambda.Compile();
        }

        private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
        {
            return method.GetParameters().Select((parameter, index) =>
              Expression.Convert(
                Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).ToArray();
        }

        public static Func<Object, Object[], Object> Bind(this MethodInfo method)
        {
            if (method.IsStatic)
            {
                if (method.ReturnType == typeof(void))
                {
                    var wrapped = CreateForVoidStaticMethod(method);
                    return (Object target, Object[] parameters) => { wrapped(parameters); return (Object)null; };
                }
                else
                {
                    var wrapped = CreateForNonVoidStaticMethod(method);
                    return (Object target, Object[] parameters) => { return wrapped(parameters); };
                }
            }
            else
            {
                if (method.ReturnType == typeof(void))
                {
                    var wrapped = CreateForVoidInstanceMethod(method);
                    return (Object target, Object[] parameters) => { wrapped(target, parameters); return (Object)null; };
                }
                else
                {
                    var wrapped = CreateForNonVoidInstanceMethod(method);
                    return (Object target, Object[] parameters) => { return wrapped(target, parameters); };
                }
            }
        }
    }
}