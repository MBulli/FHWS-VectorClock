using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using VectorClock.Common;

namespace VectorClock.Node
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Invalid parameter.\nUsage: {0} <hostID>",
                        System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                return;
            }

            int hostID = 0;

            if (!int.TryParse(args[0], out hostID))
            {
                Console.WriteLine("FATAL: Failed to parse hostID commandline parameter.");
                return;
            }

            IPEndPoint localEndpoint = NetworkConfig.NodeEndpoints[hostID];

            ApplicationLogic appLogic = new ApplicationLogic();
            CommunicationLogic commLogic = new CommunicationLogic(appLogic, hostID);
            ControlLogic controlLogic = new ControlLogic(commLogic, localEndpoint);

            Console.WriteLine($"Node with Id {hostID} and IP-address {Dns.GetHostName().ToString()} started properly.");
            Console.WriteLine($"Listening to UDP-Port: {localEndpoint.Port}");
            var causallyOrderedMode = true;
            var modeString = causallyOrderedMode ? "Causally ordered" : "Not causally ordered";
            Console.WriteLine($"Mode: {modeString}");
            Console.WriteLine("------------- Start listening to messages -------------");

            using (UdpClient client = new UdpClient(localEndpoint.Port, AddressFamily.InterNetwork))
            {
                while (true)
                {
                    IPEndPoint remoteEP = null;
                    byte[] data = client.Receive(ref remoteEP);

                    Message msg = MessageDeserializer.Deserialize(data);
                    controlLogic.HandleMessage(causallyOrderedMode, msg, remoteEP);
                }
            }
        }
    }
}
