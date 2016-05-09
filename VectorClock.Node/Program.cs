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
            using (UdpClient client = new UdpClient(1337, AddressFamily.InterNetwork))
            {
                while (true)
                {
                    IPEndPoint remoteEP = null;
                    byte[] data = client.Receive(ref remoteEP);

                    Message msg = MessageDeserializer.Deserialize(data);


                }
            }
        }
    }
}
