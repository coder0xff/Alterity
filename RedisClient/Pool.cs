using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Redis
{
    /// <summary>
    /// A thread-safe, automatically-growing pool of Clients
    /// </summary>
    public class Pool
    {
        readonly System.Net.IPAddress[] shardLocations;
        readonly LinkedList<Client> clients;

        static System.Net.IPAddress[] GetShardUris(string connectionStringName)
        {
            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            System.Configuration.ConnectionStringSettings connString =
                rootWebConfig.ConnectionStrings.ConnectionStrings[connectionStringName];
            if (connString != null)
            {
                string[] locationStrings = connString.ConnectionString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                return locationStrings.Select(x => System.Net.IPAddress.Parse(x)).ToArray();
            }
            else
                throw new InvalidOperationException("Connection string \"" + connectionStringName + "\" was not found.");
        }

        public Pool(string connectionStringName)
        {
            shardLocations = GetShardUris(connectionStringName);
            clients = new LinkedList<Client>();
        }

        public Client Get()
        {
            lock (clients)
            {
                if (clients.Count > 0)
                {
                    Client result = clients.First.Value;
                    clients.RemoveFirst();
                    return result;
                }
            }
            return new Client(shardLocations);
        }

        public void Release(Client client)
        {
            lock (clients)
            {
                clients.AddLast(client);
            }
        }
    }
}
