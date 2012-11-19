namespace System.Net
{
    public struct HostEndPoint
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public HostEndPoint(string host, int port) : this()
        {
            this.Host = host;
            this.Port = port;
        }

        public static HostEndPoint Parse(string location, int defaultPort)
        {
            string hostName = location;
            int port = defaultPort;
            int colonIndex = location.IndexOf(':');
            if (colonIndex >= 0)
            {
                hostName = location.Substring(0, colonIndex);
                port = Convert.ToInt32(location.Substring(colonIndex + 1));
            }
            return new HostEndPoint(hostName, port);
        }
    }
}
