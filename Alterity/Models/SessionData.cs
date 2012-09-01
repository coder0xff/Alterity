using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using Alterity.Controllers;

namespace Alterity.Models
{
    public class SessionData : DynamicObject
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public DateTime Expiration { get; set; }
        public byte[] Data { get; set; }

        private Dictionary<String, Object> cache;
        private static BinaryFormatter formatter = new BinaryFormatter();

        private void CacheLoad()
        {
            if (Data == null)
            {
                cache = new Dictionary<string, object>();
            }
            else
            {
                var tempStream = new System.IO.MemoryStream();
                tempStream.Write(Data, 0, Data.Length);
                tempStream.Seek(0, System.IO.SeekOrigin.Begin);
                cache = (Dictionary<String, Object>)formatter.Deserialize(tempStream);
            }
        }

        private void CacheStore()
        {
            var tempStream = new System.IO.MemoryStream();
            formatter.Serialize(tempStream, cache);
            Data = tempStream.ToArray();
        }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            return null;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (cache == null) CacheLoad();
            if (!cache.TryGetValue(binder.Name, out result))
                result = GetDefault(binder.ReturnType);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (value == null)
                cache.Remove(binder.Name);
            else
                cache[binder.Name] = value;
            CacheStore();
            return true;
        }
    }

    internal class SessionDataWrapper
    {
        const String cookieName = "persistedSession";
        const int sessionExpirationMinutes = 480;
        static SessionData CreateSessionData(Guid guid)
        {
            SessionData sessionData = new SessionData();
            sessionData.Id = guid;
            sessionData.Expiration = DateTime.Now.AddMinutes(sessionExpirationMinutes);
            sessionData = EntityMappingContext.Current.SessionDatas.Add(sessionData);
            EntityMappingContext.Current.SaveChanges();
            return sessionData;
        }

        public static dynamic GetSessionData(HttpRequest request, HttpResponse response)
        {
            HttpCookie persistedSessionCookie = request.Cookies[cookieName];
            if (persistedSessionCookie == null)
                persistedSessionCookie = new HttpCookie(cookieName, Guid.NewGuid().ToString());
            Guid sessionGuid = Guid.Parse(persistedSessionCookie.Value);
            SessionData data = EntityMappingContext.Current.SessionDatas.FirstOrDefault(x => x.Id == sessionGuid);
            if (data == null)
                data = CreateSessionData(Guid.Parse(persistedSessionCookie.Value));
            persistedSessionCookie.Expires = DateTime.Now.AddMinutes(sessionExpirationMinutes);
            data.Expiration = persistedSessionCookie.Expires;
            if (response != null) response.AppendCookie(persistedSessionCookie);
            return data;
        }
    }
}

