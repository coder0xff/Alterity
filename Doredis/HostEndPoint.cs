// ReSharper disable CheckNamespace

namespace System.Net
// ReSharper restore CheckNamespace
{
    public struct HostEndPoint
    {
        public HostEndPoint(string host, int port) : this()
        {
            Host = host;
            Port = port;
        }

        public string Host { get; private set; }
        public int Port { get; private set; }

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

        public static bool operator ==(HostEndPoint lhs, HostEndPoint rhs)
        {
            return lhs.Host == rhs.Host && lhs.Port == rhs.Port;
        }

        public static bool operator !=(HostEndPoint lhs, HostEndPoint rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            if ((Object) this == obj) return true;
            if (obj is HostEndPoint)
            {
                var other = (HostEndPoint) obj;
                return this == other;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Utility.CombineHashCodes(Host, Port);
        }
    }
}