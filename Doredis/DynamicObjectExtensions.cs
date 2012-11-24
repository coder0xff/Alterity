using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            MethodInfo method;
            Type lambdaType;
            public MethodWrapper(MethodInfo method)
            {
                lambdaType = method.LambdaType();
                this.method = method;
            }

            public bool TryGet(object target, out object result)
            {
                result = method.CreateDelegate(lambdaType, target);
                return true;
            }

            public bool TrySet(object target, object value)
            {
                return false;
            }
        }

        class PropertyWrapper : IObjectWrapper
        {
            PropertyInfo property;

            public PropertyWrapper(PropertyInfo property)
            {
                this.property = property;
            }

            public bool TryGet(object target, out Object result)
            {
                result = null;
                if (!property.CanRead) return false;
                result = property.GetValue(target);
                return true;
            }

            public bool TrySet(object target, object value)
            {
                if (!property.CanWrite) return false;
                property.SetValue(target, value);
                return true;
            }
        }

        class FieldWrapper : IObjectWrapper
        {
            FieldInfo field;

            public FieldWrapper(FieldInfo field)
            {
                this.field = field;
            }

            public bool TryGet(object target, out object result)
            {
                result = field.GetValue(target);
                return true;
            }

            public bool TrySet(object target, object value)
            {
                field.SetValue(target, value);
                return true;
            }
        }

        static Dictionary<Type, Dictionary<string, IObjectWrapper>> membersByType = new Dictionary<Type, Dictionary<string, IObjectWrapper>>();
        static Dictionary<Type, Dictionary<string, IObjectWrapper>> caseInsensitiveMembersByType = new Dictionary<Type, Dictionary<string, IObjectWrapper>>();

        static void AddWrapperForType(Type t, string name, IObjectWrapper wrapper)
        {
            try
            {
                membersByType[t].Add(name, wrapper);
                caseInsensitiveMembersByType[t].Add(name, wrapper);
            }
            catch (ArgumentException exc)
            {
                throw new ArgumentException("The type \"" + t.FullName + "\" has multiple members named \"" + name + "\" (case insensitive). Self accessing dynamic-objects cannot support this.", exc);
            }
        }

        static void InitForType(Type t)
        {
            if (membersByType.ContainsKey(t))
            {
                return;
            }
            else
            {
                membersByType[t] = new Dictionary<string,IObjectWrapper>();
                caseInsensitiveMembersByType[t] = new Dictionary<string,IObjectWrapper>();

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
                if (caseInsensitiveMembersByType[thisType].TryGetValue(binder.Name.ToLowerInvariant(), out wrapper))
                {
                    return wrapper.TryGet(self, out result);
                }
                return false;
            }
            else
            {
                IObjectWrapper wrapper;
                if (membersByType[thisType].TryGetValue(binder.Name, out wrapper))
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
                if (caseInsensitiveMembersByType[thisType].TryGetValue(binder.Name.ToLowerInvariant(), out wrapper))
                {
                    return wrapper.TrySet(self, value);
                }
                return false;
            }
            else
            {
                IObjectWrapper wrapper;
                if (membersByType[thisType].TryGetValue(binder.Name, out wrapper))
                {
                    return wrapper.TrySet(self, value);
                }
                return false;
            }
        }
    }
}
