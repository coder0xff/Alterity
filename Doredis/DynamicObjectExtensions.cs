using System;
using System.Collections.Generic;
using FastDelegate;
using System.Reflection;
using System.Dynamic;

namespace Doredis
{
    /// <summary>
    /// Helps to make classes that work like DynamicObject,
    /// but want to provide dynamic access to their statically typed members
    /// </summary>
    internal static class DynamicObjectExtensions
    {
        interface IObjectWrapper
        {
            bool TryGet(object target, out object value);
            bool TrySet(object target, object value);
        }

        class MethodWrapper : IObjectWrapper
        {
            readonly MethodInfo _method;
            readonly Type _lambdaType;
            public MethodWrapper(MethodInfo method)
            {
                _lambdaType = method.LambdaType();
                _method = method;
            }

            public bool TryGet(object target, out object result)
            {
                result = _method.CreateDelegate(_lambdaType, target);
                return true;
            }

            public bool TrySet(object target, object value)
            {
                return false;
            }
        }

        class PropertyWrapper : IObjectWrapper
        {
            readonly PropertyInfo _property;

            public PropertyWrapper(PropertyInfo property)
            {
                _property = property;
            }

            public bool TryGet(object target, out Object result)
            {
                result = null;
                if (!_property.CanRead) return false;
                result = _property.GetValue(target);
                return true;
            }

            public bool TrySet(object target, object value)
            {
                if (!_property.CanWrite) return false;
                _property.SetValue(target, value);
                return true;
            }
        }

        class FieldWrapper : IObjectWrapper
        {
            readonly FieldInfo _field;

            public FieldWrapper(FieldInfo field)
            {
                _field = field;
            }

            public bool TryGet(object target, out object result)
            {
                result = _field.GetValue(target);
                return true;
            }

            public bool TrySet(object target, object value)
            {
                _field.SetValue(target, value);
                return true;
            }
        }

        static readonly Dictionary<Type, Dictionary<string, IObjectWrapper>> MembersByType = new Dictionary<Type, Dictionary<string, IObjectWrapper>>();
        static readonly Dictionary<Type, Dictionary<string, IObjectWrapper>> CaseInsensitiveMembersByType = new Dictionary<Type, Dictionary<string, IObjectWrapper>>();

        static void AddWrapperForType(Type t, string name, IObjectWrapper wrapper)
        {
            try
            {
                MembersByType[t].Add(name, wrapper);
                CaseInsensitiveMembersByType[t].Add(name, wrapper);
            }
            catch (ArgumentException exc)
            {
                throw new ArgumentException("The type \"" + t.FullName + "\" has multiple members named \"" + name + "\" (case insensitive). Self accessing dynamic-objects cannot support this.", exc);
            }
        }

        static void InitForType(Type t)
        {
            if (MembersByType.ContainsKey(t))
            {
            }
            else
            {
                MembersByType[t] = new Dictionary<string,IObjectWrapper>();
                CaseInsensitiveMembersByType[t] = new Dictionary<string,IObjectWrapper>();

                foreach (MethodInfo methodInfo in t.GetMethods(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance))
                {
                    if (methodInfo.IsSpecialName) continue;

                    bool unusableParameters = false;
                    foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
                    {
                        if (parameterInfo.IsOut) unusableParameters = true;
                        if (unusableParameters) break;
                    }
                    if (unusableParameters) continue;

                    AddWrapperForType(t, methodInfo.Name, new MethodWrapper(methodInfo));
                }

                foreach(PropertyInfo propertyInfo in t.GetProperties(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance))
                    if (!propertyInfo.IsSpecialName)
                        AddWrapperForType(t, propertyInfo.Name, new PropertyWrapper(propertyInfo));

                foreach(FieldInfo fieldInfo in t.GetFields(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance))
                    if (!fieldInfo.IsSpecialName)
                        AddWrapperForType(t, fieldInfo.Name, new FieldWrapper(fieldInfo));
            }
        }

        public static bool TryGetStaticallyTypedMember(this DynamicObject self, GetMemberBinder binder, out object result)
        {
            result = null;
            Type thisType = self.GetType();
            InitForType(thisType);
            if (binder.IgnoreCase)
            {
                IObjectWrapper wrapper;
                if (CaseInsensitiveMembersByType[thisType].TryGetValue(binder.Name.ToLowerInvariant(), out wrapper))
                {
                    return wrapper.TryGet(self, out result);
                }
                return false;
            }
            else
            {
                IObjectWrapper wrapper;
                if (MembersByType[thisType].TryGetValue(binder.Name, out wrapper))
                {
                    return wrapper.TryGet(self, out result);
                }
                return false;
            }
        }

        public static bool TrySetStaticallyTypedMember(this DynamicObject self, SetMemberBinder binder, object value)
        {
            Type thisType = self.GetType();
            InitForType(thisType);
            if (binder.IgnoreCase)
            {
                IObjectWrapper wrapper;
                if (CaseInsensitiveMembersByType[thisType].TryGetValue(binder.Name.ToLowerInvariant(), out wrapper))
                {
                    return wrapper.TrySet(self, value);
                }
                return false;
            }
            else
            {
                IObjectWrapper wrapper;
                if (MembersByType[thisType].TryGetValue(binder.Name, out wrapper))
                {
                    return wrapper.TrySet(self, value);
                }
                return false;
            }
        }
    }
}
