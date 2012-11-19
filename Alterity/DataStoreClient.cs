using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Doredis;

namespace Alterity
{
    public class DataStoreClient
    {
        static DataStore dataStore;

        static System.Net.HostEndPoint[] GetShardEndPoints(string connectionStringName)
        {
            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            System.Configuration.ConnectionStringSettings connString =
                rootWebConfig.ConnectionStrings.ConnectionStrings[connectionStringName];
            if (connString != null)
            {
                string[] locationStrings = connString.ConnectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                return locationStrings.Select(x => System.Net.HostEndPoint.Parse(x, 6379)).ToArray();
            }
            else
                throw new InvalidOperationException("Connection string \"" + connectionStringName + "\" was not found.");
        }

        static DataStoreClient()
        {
            dataStore = new DataStore(GetShardEndPoints("DataStore"));
        }

        public static DataStore Get()
        {
            return dataStore;
        }
    }
}