using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
        private Random random = new Random(0);
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

                    TextBoxContent += string.Format("{0:HH:mm:ss}: Answer from {1}. Message: {2}\n Balance: {3} Clock: {4}\n", 
                        DateTime.Now, 
                        result.RemoteEndPoint, 
                        msg.controlBlock.Command, 
                        msg.communicationBlock.payload.balance,
                        msg.communicationBlock.clock);

                    NodeFromMessage(msg).CurrentBalance = msg.communicationBlock.payload.balance;
                    NodeFromMessage(msg).CurrentClock = msg.communicationBlock.clock.ToString();
                }
            }
        }

        private NodeViewModel NodeFromMessage(Message msg)
        {
            switch (msg.communicationBlock.clock.getID())
            {
                case 0: return Node1;
                case 1: return Node2;
                case 2: return Node3;
                default: throw new InvalidOperationException($"Invalid node id '{msg.communicationBlock.clock.getID()}'");
            }
        }

        public void CheckNodeConnectivities()
        {
            Task.Run(async () => await Node1.CheckConnectivity());
            Task.Run(async () => await Node2.CheckConnectivity());
            Task.Run(async () => await Node3.CheckConnectivity());
        }

        private RelayCommand testCommand;
        public RelayCommand TestCommand
        {
            get
            {
                if (testCommand == null)
                {
                    testCommand = new RelayCommand(async () =>
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            int delta = random.Next(1, 100);
                            Message msg = MessageFactory.Control.CreateUpdateControlMessage(delta);

                            int node = random.Next(1, 4);

                            switch (node)
                            {
                                case 1:
                                    await Node1.SendMessageAsync(msg);
                                    break;
                                case 2:
                                    await Node2.SendMessageAsync(msg);
                                    break;
                                case 3:
                                    await Node3.SendMessageAsync(msg);
                                    break;
                            }

                            Thread.Sleep(100);
                        }
                    });
                }
                return testCommand;
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
