using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;

namespace Alterity.Models
{
    public class SessionData
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public DateTime Expiration { get; set; }
        [Required]
        public byte[] Data { get; set; }
    }

    internal class SessionDataWrapper
    {
        private SessionData Data;
        private ExpandoObject expando = new ExpandoObject();
        private static BinaryFormatter formatter = new BinaryFormatter();
        internal SessionDataWrapper(SessionData data)
        {
            this.Data = data;
            var tempStream = new System.IO.MemoryStream();
            tempStream.Write(Data.Data, 0, Data.Data.Length);
            Dictionary<String, Object> dictionary = (Dictionary<String, Object>)formatter.Deserialize(tempStream);
            foreach (KeyValuePair<String, Object> entry in dictionary)
                ((IDictionary<string, Object>)expando).Add(entry);
            ((INotifyPropertyChanged)expando).PropertyChanged +=
            new PropertyChangedEventHandler(HandlePropertyChanges);
        }

        private void HandlePropertyChanges(Object sender, PropertyChangedEventArgs e)
        {
            Dictionary<String, Object> dictionary = new Dictionary<String, Object>();
            foreach (KeyValuePair<String, Object> entry in (IDictionary<String, Object>)expando)
                dictionary.Add(entry.Key, entry.Value);
            var tempStream = new System.IO.MemoryStream();
            formatter.Serialize(tempStream, dictionary);
            Data.Data = tempStream.ToArray();
        }

        internal dynamic GetExpando()
        {
            return expando;
        }
    }

    public static class SessionDataExtensionsMethods
    {
        const String cookieName = "persistedSession";
        const int sessionExpirationMinutes = 480;
        static SessionData CreateSessionData(EntityMappingContext dbContext)
        {
            SessionData sessionData = new SessionData();
            sessionData.Id = Guid.NewGuid();
            sessionData.Expiration = DateTime.Now.AddHours(sessionExpirationMinutes);
            Dictionary<String, Object> dictionary = new Dictionary<String, Object>();
            var tempStream = new System.IO.MemoryStream();
            new BinaryFormatter().Serialize(tempStream, dictionary);
            sessionData.Data = tempStream.ToArray();
            sessionData = dbContext.SessionData.Add(sessionData);
            return sessionData;
        }

        public static dynamic GetSessionData(this HttpRequestBase httpRequest, EntityMappingContext dbContext)
        {
            HttpCookie persistedSessionCookie = httpRequest.Cookies[cookieName];
            SessionData data;
            if (persistedSessionCookie == null)
            {
                data = CreateSessionData(dbContext);
                persistedSessionCookie = new HttpCookie(cookieName, data.Id.ToString());
                persistedSessionCookie.Expires = data.Expiration;
            }
            else
            {
                data = dbContext.SessionData.FirstOrDefault(x => x.Id == Guid.Parse(persistedSessionCookie.Value));
                if (data == null)
                {
                    data = CreateSessionData(dbContext);
                    persistedSessionCookie = new HttpCookie(cookieName, data.Id.ToString());
                    persistedSessionCookie.Expires = data.Expiration;
                }
                persistedSessionCookie.Expires = DateTime.Now.AddHours(sessionExpirationMinutes);
                data.Expiration = persistedSessionCookie.Expires;
            }
            SessionDataWrapper wrapper = new SessionDataWrapper(data);
            return wrapper.GetExpando();
        }
    }
}