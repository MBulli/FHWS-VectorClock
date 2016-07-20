using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VectorClock.Common
{
    public class NetworkConfig
    {
        public static IPEndPoint CommanderEndpoint = new IPEndPoint(IPAddress.Loopback, 1340);

        public static IPEndPoint[] NodeEndpoints = new IPEndPoint[]
        {
            new IPEndPoint(IPAddress.Loopback, 1337),
            new IPEndPoint(IPAddress.Loopback, 1338),
            new IPEndPoint(IPAddress.Loopback, 1339)
        };

        //public static IPEndPoint CommanderEndpoint = new IPEndPoint(IPAddress.Parse("10.10.29.21"), 1340);

        //public static IPEndPoint[] NodeEndpoints = new IPEndPoint[]
        //{
        //    new IPEndPoint(IPAddress.Parse("10.10.29.21"), 1337),
        //    new IPEndPoint(IPAddress.Parse("10.10.29.142"), 1338),
        //    new IPEndPoint(IPAddress.Parse("10.10.29.67"), 1339)
        //};
    }
}
