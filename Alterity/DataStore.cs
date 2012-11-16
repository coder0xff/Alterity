using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;

namespace Alterity
{
    public class DataStore
    {
        static Doredis.Pool redisPool;

        static DataStore()
        {
            redisPool = new Doredis.Pool("DataStore");
        }
        
        public static dynamic Access()
        {
            return new DS(redisPool);
        }
    }

    public class DS : DynamicObject, IDisposable
    {
        readonly Doredis.Client client;
        readonly Doredis.Pool pool;
        public DS(Doredis.Pool pool)
        {
            this.pool = pool;
            client = pool.Get();
        }

        public void Dispose()
        {
            pool.Release(client);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return client.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return client.TrySetMember(binder, value);
        }

        public object this[string name]
        {
            get
            {
                return client[name];
            }
            set
            {
                client[name] = value;
            }
        }

    }
}