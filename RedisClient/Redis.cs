using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisClient
{
    public class Connection
    {
        public Connection(string connectionStringName)
        {

        }

        static Uri GetConnectionString(string connectionStringName)
        {
            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            System.Configuration.ConnectionStringSettings connString =
                rootWebConfig.ConnectionStrings.ConnectionStrings[connectionStringName];
            if (connString != null)
                return new Uri(connString.ConnectionString);
            else
                throw new InvalidOperationException("Connection string \"" + connectionStringName + "\" was not found.");
        }
    }
}
