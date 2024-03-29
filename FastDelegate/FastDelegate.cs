﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FastDelegate
{
    public static class MethodInfoExtensions
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

        public static Type LambdaType(this MethodInfo method)
        {
            if (method.ReturnType == typeof(void))
            {
                Type actionGenericType = null;
                switch (method.GetParameters().Length)
                {
                    case 0:
                        return typeof(Action);
                    case 1:
                        actionGenericType = typeof(Action<>);
                        break;
                    case 2:
                        actionGenericType = typeof(Action<,>);
                        break;
                    case 3:
                        actionGenericType = typeof(Action<,,>);
                        break;
                    case 4:
                        actionGenericType = typeof(Action<,,,>);
                        break;
                    case 5:
                        actionGenericType = typeof(Action<,,,,>);
                        break;
                    case 6:
                        actionGenericType = typeof(Action<,,,,,>);
                        break;
                    case 7:
                        actionGenericType = typeof(Action<,,,,,,>);
                        break;
                    case 8:
                        actionGenericType = typeof(Action<,,,,,,,>);
                        break;
                    case 9:
                        actionGenericType = typeof(Action<,,,,,,,,>);
                        break;
                    case 10:
                        actionGenericType = typeof(Action<,,,,,,,,,>);
                        break;
                    case 11:
                        actionGenericType = typeof(Action<,,,,,,,,,,>);
                        break;
                    case 12:
                        actionGenericType = typeof(Action<,,,,,,,,,,,>);
                        break;
                    case 13:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,>);
                        break;
                    case 14:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,,>);
                        break;
                    case 15:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,,,>);
                        break;
                    case 16:
                        actionGenericType = typeof(Action<,,,,,,,,,,,,,,,>);
                        break;
                    default:
                        throw new NotSupportedException("Lambdas may only have up to 16 parameters.");
                }
                return actionGenericType.MakeGenericType(method.GetParameters().Select(_ => _.ParameterType).ToArray());
            }
            else
            {
                Type funcGenericType = null;
                switch (method.GetParameters().Length)
                {
                    case 0:
                        return typeof(Func<>);
                    case 1:
                        funcGenericType = typeof(Func<,>);
                        break;
                    case 2:
                        funcGenericType = typeof(Func<,,>);
                        break;
                    case 3:
                        funcGenericType = typeof(Func<,,,>);
                        break;
                    case 4:
                        funcGenericType = typeof(Func<,,,,>);
                        break;
                    case 5:
                        funcGenericType = typeof(Func<,,,,,>);
                        break;
                    case 6:
                        funcGenericType = typeof(Func<,,,,,,>);
                        break;
                    case 7:
                        funcGenericType = typeof(Func<,,,,,,,>);
                        break;
                    case 8:
                        funcGenericType = typeof(Func<,,,,,,,,>);
                        break;
                    case 9:
                        funcGenericType = typeof(Func<,,,,,,,,,>);
                        break;
                    case 10:
                        funcGenericType = typeof(Func<,,,,,,,,,,>);
                        break;
                    case 11:
                        funcGenericType = typeof(Func<,,,,,,,,,,,>);
                        break;
                    case 12:
                        funcGenericType = typeof(Func<,,,,,,,,,,,,>);
                        break;
                    case 13:
                        funcGenericType = typeof(Func<,,,,,,,,,,,,,>);
                        break;
                    case 14:
                        funcGenericType = typeof(Func<,,,,,,,,,,,,,,>);
                        break;
                    case 15:
                        funcGenericType = typeof(Func<,,,,,,,,,,,,,,,>);
                        break;
                    case 16:
                        funcGenericType = typeof(Func<,,,,,,,,,,,,,,,,>);
                        break;
                    default:
                        throw new NotSupportedException("Lambdas may only have up to 16 parameters.");
                }
                Type[] parametersAndReturnType = new Type[method.GetParameters().Length + 1];
                method.GetParameters().Select(_ => _.ParameterType).ToArray().CopyTo(parametersAndReturnType, 0);
                parametersAndReturnType[parametersAndReturnType.Length - 1] = method.ReturnType;
                return funcGenericType.MakeGenericType(parametersAndReturnType);
            }
        }
    }

}
