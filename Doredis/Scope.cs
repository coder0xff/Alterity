﻿using System;
using System.Dynamic;
namespace Doredis
{
    internal class Scope : DynamicDataObject
    {
        readonly string absolutePath;
        readonly DataStoreShard dataStore;

        internal Scope(string absolutePath, DataStoreShard dataStore)
        {
            this.absolutePath = absolutePath;
            this.dataStore = dataStore;
        }

        public override DataStoreShard GetDataStore(string memberAbsolutePath)
        {
            return dataStore;
        }

        public override string GetMemberAbsolutePath(string name, bool ignoreCase)
        {
            return absolutePath + "." + (ignoreCase ? name.ToLowerInvariant() : name);
        }

        internal override Scope CreateScope(string memberAbsolutePath)
        {
            return new Scope(memberAbsolutePath , dataStore);
        }

        public override string GetAbsolutePath()
        {
            return absolutePath;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder.ReturnType == typeof(Int32))
            {
                dataStore.Set<Int32>(absolutePath, (Int32)value);
                return true;
            }
            return base.TrySetMember(binder, value);
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = null;
            if (binder.ReturnType == typeof(Int32))
            {
                result = dataStore.Get<Int32>(absolutePath);
                return true;
            }
            return false;
        }
    }
}