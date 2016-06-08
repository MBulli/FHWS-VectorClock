using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VectorClock.Commander.Helper;
using VectorClock.Common;

namespace VectorClock.Commander.ViewModel
{
    public class MainViewModel : PropertyChangedBase
    {
        private string title = "VectorClock";
        public string Title
        {
            get { return title; } 
            set { title = value; OnNotifyPropertyChanged(); }
        }

        private string textBoxContent = "";
        public string TextBoxContent
        {
            get { return textBoxContent; }
            set { textBoxContent = value; OnNotifyPropertyChanged(); }
        }


        public NodeViewModel Node1 { get; set; }
        public NodeViewModel Node2 { get; set; }
        public NodeViewModel Node3 { get; set; }

        //public NodeViewModel NodeLokal { get; set; }

        public MainViewModel()
        {
            //Node1 = new NodeViewModel("mjverteil01", System.Net.IPAddress.Parse("10.10.29.21"));
            //Node2 = new NodeViewModel("mjverteil02", System.Net.IPAddress.Parse("10.10.29.142"));
            //Node3 = new NodeViewModel("mjverteil03", System.Net.IPAddress.Parse("10.10.29.67"));

            // this is for one-machine testing only
            Node1 = new NodeViewModel("mjverteil01", new IPEndPoint(IPAddress.Loopback, 1337));
            Node2 = new NodeViewModel("mjverteil02", new IPEndPoint(IPAddress.Loopback, 1338));
            Node3 = new NodeViewModel("mjverteil03", new IPEndPoint(IPAddress.Loopback, 1339));

            //NodeLokal = new NodeViewModel("lokal", System.Net.IPAddress.Loopback);

            CheckNodeConnectivities(); //TODO: Timer

            Task listenToNodeTask = new Task(async () => await ListenToNode());
            listenToNodeTask.Start();
        }

        public async Task ListenToNode()
        {
            using (UdpClient client = new UdpClient(1340, AddressFamily.InterNetwork))
            {
                while (true)
                {
                    UdpReceiveResult result = await client.ReceiveAsync();
                               
                    Message msg = MessageDeserializer.Deserialize(result.Buffer);
                    TextBoxContent += $"Answer from: { result.RemoteEndPoint.Address}:{result.RemoteEndPoint.Port}. Message: {msg.controlBlock.Command} \n Balance: {msg.communicationBlock.payload.balance} Clock: {msg.communicationBlock.clock}\n" ;
                }
            }
        }

        public void CheckNodeConnectivities()
        {
            Task.Run(async () => await Node1.CheckConnectivity());
            Task.Run(async () => await Node2.CheckConnectivity());
            Task.Run(async () => await Node3.CheckConnectivity());
        }

        private RelayCommand startCommand;
        public RelayCommand StartCommand //buttonclick
        {
            get
            {
                if(startCommand == null)
                {
                    startCommand = new RelayCommand(async () =>
                    {
                        Message msg = new Message();

                        msg.controlBlock.Command = ControlCommand.IncreaseBalance;
                        msg.communicationBlock.payload.balance = 10;

                        await Task.WhenAll(
                                        Node1.SendMessageAsync(msg),
                                        Node2.SendMessageAsync(msg),
                                        Node3.SendMessageAsync(msg));

                        Message msg2 = new Message();

                        msg2.controlBlock.Command = ControlCommand.SendMessageTo;
                        msg2.communicationBlock.payload.port = 1338;

                        await Task.WhenAll(Node1.SendMessageAsync(msg2));
                    });
                }
                return startCommand;
            }
        }

        private RelayCommand resetBalanceCommand;
        public ICommand ResetBalanceCommand
        {
            get
            {
                if (resetBalanceCommand == null)
                {
                    resetBalanceCommand = new RelayCommand(async () =>
                    {
                        Message msg = new Message();

                        msg.controlBlock.Command = ControlCommand.SetBalance;
                        msg.controlBlock.BalanceDelta = 0;

                        await Task.WhenAll(
                                        Node1.SendMessageAsync(msg),
                                        Node2.SendMessageAsync(msg),
                                        Node3.SendMessageAsync(msg));
                    });
                }

                return resetBalanceCommand;
            }
        }

    }
}
