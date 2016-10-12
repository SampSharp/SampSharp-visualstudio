using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SampSharp.VisualStudio.Debugger
{
    public class DebuggerAddress
    {
        public static IPAddress LocalIp = new IPAddress(0);

        public DebuggerAddress(IPAddress ip, ushort port)
        {
            if (ip == null) throw new ArgumentNullException(nameof(ip));
            Ip = ip;
            Port = port;
        }

        public DebuggerAddress(ushort port)
        {
            Ip = LocalIp;
            Port = port;
        }

        public IPAddress Ip { get; }

        public ushort Port { get; }

        public override string ToString()
        {
            return $"{Ip}:{Port}";
        }

        public static DebuggerAddress GetAvailable()
        {
            return new DebuggerAddress(LocalIp, (ushort) FreeTcpPort());
        }

        private static int FreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            var port = ((IPEndPoint) l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public static bool TryParse(string s, out DebuggerAddress result)
        {
            if (s == null)
            {
                result = null;
                return false;
            }

            var split = s.Split(':');

            if (split.Length != 2)
            {
                result = null;
                return false;
            }

            IPAddress ip;
            if (!IPAddress.TryParse(split[0], out ip))
            {
                result = null;
                return false;
            }

            ushort port;
            if (!ushort.TryParse(split[1], out port))
            {
                result = null;
                return false;
            }

            result = new DebuggerAddress(ip, port);
            return true;
        }
    }
}