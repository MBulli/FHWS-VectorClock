using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using VectorClock.Commander.Helper;
using VectorClock.Common;

namespace VectorClock.Commander.ViewModel
{
    public class NodeViewModel : PropertyChangedBase
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; OnNotifyPropertyChanged(); }
        }

        private IPAddress ipAddress;
        public IPAddress IpAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; OnNotifyPropertyChanged(); }
        }

        private bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
            set { isConnected = value; OnNotifyPropertyChanged(); }
        }



        public NodeViewModel(string name, IPAddress ipAddress)
        {
            this.name = name;
            this.ipAddress = ipAddress;
        }

        public async Task CheckConnectivity()
        {
            using (var ping = new Ping())
            {
                var reply = await ping.SendPingAsync(this.IpAddress);
                this.IsConnected = reply.Status == IPStatus.Success;
            }
        }

        public async Task sendMessage(Message msg, IPAddress destination)
        {
            using (UdpClient client = new UdpClient())
            {
                client.Connect(destination, 1337);
                byte[] data = MessageSerializer.Serialze(msg);
                client.Send(data, data.Length);
            }
        }
    }
}
