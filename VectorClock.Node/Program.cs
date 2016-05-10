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
            switch (Dns.GetHostName().ToString())
            {
                case "10.10.29.21":
                    hostID = 0;
                    break;
                case "10.10.29.142":
                    hostID = 1;
                    break;
                case "10.10.29.67":
                    hostID = 2;
                    break;
            }

            
            ApplicationLogic appLogic = new ApplicationLogic();
            CommunicationLogic commLogic = new CommunicationLogic(appLogic, hostID);
            ControlLogic controlLogic = new ControlLogic(commLogic);

            using (UdpClient client = new UdpClient(1337, AddressFamily.InterNetwork))
            {
                while (true)
                {
                    IPEndPoint remoteEP = null;
                    byte[] data = client.Receive(ref remoteEP);

                    Message msg = MessageDeserializer.Deserialize(data);
                    controlLogic.HandleMessage(msg);
                }
            }
        }
    }
}
