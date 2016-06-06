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
            int hostID = -1;
            int port = 1337;

            Console.Write("Insert port to listen to: ");
            port = Convert.ToInt32(Console.ReadLine());

            //switch (Dns.GetHostName().ToString())
            //{
            //    case "10.10.29.21":
            //        hostID = 0;
            //        break;
            //    case "10.10.29.142":
            //        hostID = 1;
            //        break;
            //    case "10.10.29.67":
            //        hostID = 2;
            //        break;
            //}

            //Console.WriteLine("Node with Id " + hostID + " and IP-address " + Dns.GetHostName().ToString() + " startet properly.");

            // for one-machine testing only
            switch (port)
            {
                case 1337:
                    hostID = 0;
                    break;
                case 1338:
                    hostID = 1;
                    break;
                case 1339:
                    hostID = 2;
                    break;
            }

            ApplicationLogic appLogic = new ApplicationLogic();
            CommunicationLogic commLogic = new CommunicationLogic(appLogic, hostID);
            ControlLogic controlLogic = new ControlLogic(commLogic);

            Console.WriteLine("Node with Id " + hostID + " and IP-address " + Dns.GetHostName().ToString() + " startet properly.");
            Console.WriteLine("Listening to UDP-Port: " + port);

            using (UdpClient client = new UdpClient(port, AddressFamily.InterNetwork))
            {
                while (true)
                {
                    if (client.Available > 0) // Only read if we have some data 
                    {
                        IPEndPoint remoteEP = null;
                        byte[] data = client.Receive(ref remoteEP);

                        Message msg = MessageDeserializer.Deserialize(data);
                        controlLogic.HandleMessage(msg, remoteEP);
                    }
                }
            }
        }
    }
}
