using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

        private IPEndPoint ipEndpoint;
        public IPEndPoint IpEndpoint
        {
            get { return ipEndpoint; }
            set { ipEndpoint = value; OnNotifyPropertyChanged(); }
        }

        private bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
            set { isConnected = value; OnNotifyPropertyChanged(); }
        }

        private decimal currentBalance;
        public decimal CurrentBalance
        {
            get { return currentBalance; }
            set { currentBalance = value; OnNotifyPropertyChanged(); }
        }

        private string balanceDeltaText;
        public string BalanceDeltaText
        {
            get { return balanceDeltaText; }
            set { balanceDeltaText = value; OnNotifyPropertyChanged(); }
        }

        private RelayCommand updateBalanceCommand;
        public ICommand UpdateBalanceCommand
        {
            get { return updateBalanceCommand; }
        }


        public NodeViewModel(string name, IPEndPoint ipAddress)
        {
            this.name = name;
            this.ipEndpoint = ipAddress;

            updateBalanceCommand = new RelayCommand(UpdateBalance);
        }

        public async Task CheckConnectivity()
        {
            using (var ping = new Ping())
            {
                var reply = await ping.SendPingAsync(this.IpEndpoint.Address);
                this.IsConnected = reply.Status == IPStatus.Success;
            }
        }

        /// <summary>
        /// Sends a message async to this node
        /// </summary>
        public async Task SendMessageAsync(Message msg)
        {
            using (UdpClient client = new UdpClient())
            {
                client.Connect(IpEndpoint);
                byte[] data = MessageSerializer.Serialze(msg);

                await client.SendAsync(data, data.Length);
            }
        }

        private async void UpdateBalance()
        {
            decimal delta = 0;

            if (decimal.TryParse(BalanceDeltaText, out delta))
            {
                Message msg = new Message();
                msg.type = MessageType.ControlCommand;
                msg.controlBlock.Command = delta < 0 ? ControlCommand.DecreaseBalance : ControlCommand.IncreaseBalance;
                msg.controlBlock.BalanceDelta = Math.Abs(delta);

                BalanceDeltaText = null;
                await SendMessageAsync(msg);
            }
        }
    }
}
