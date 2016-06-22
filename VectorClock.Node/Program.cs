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
            if (args.Length != 2)
            {
                Console.WriteLine("Invalid parameter.\nUsage: {0} <hostID> <port>",
                        System.IO.Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location));
                return;
            }

            int hostID = 0;
            int port = 0;

            if (!int.TryParse(args[0], out hostID))
            {
                Console.WriteLine("FATAL: Failed to parse hostID commandline parameter.");
                return;
            }
            if (!int.TryParse(args[1], out port))
            {
                Console.WriteLine("FATAL: Failed to parse port commandline parameter.");
                return;
            }

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port);

            ApplicationLogic appLogic = new ApplicationLogic();
            CommunicationLogic commLogic = new CommunicationLogic(appLogic, hostID);
            ControlLogic controlLogic = new ControlLogic(commLogic, endPoint);

            Console.WriteLine($"Node with Id {hostID} and IP-address {Dns.GetHostName().ToString()} started properly.");
            Console.WriteLine($"Listening to UDP-Port: {port}");
            Console.WriteLine("------------- Start listening to messages -------------");

            using (UdpClient client = new UdpClient(port, AddressFamily.InterNetwork))
            {
                while (true)
                {
                    IPEndPoint remoteEP = null;
                    byte[] data = client.Receive(ref remoteEP);

                    Message msg = MessageDeserializer.Deserialize(data);
                    controlLogic.HandleMessage(true, msg, remoteEP);
                }
            }
        }
    }
}
