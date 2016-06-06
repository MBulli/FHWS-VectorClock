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
    class ControlLogic
    {
        CommunicationLogic commLogic;

        public ControlLogic(CommunicationLogic commLogic)
        {
            this.commLogic = commLogic;
        }

        public bool HandleMessage(Message msg, IPEndPoint remoteEP)
        {
            msg.communicationBlock = new Message.CommunicationBlock(); // CommBlock from commander-message was empty
            msg.communicationBlock.clock = this.commLogic.clock;
            AnswerHost(msg);

            if (msg.controlBlock.Command == ControlCommand.Shutdown)
            {
                Console.WriteLine("Shutdown command received!");
                return true;
            }
            else if (msg.controlBlock.Command == ControlCommand.IncreaseBalance)
            {
                Console.WriteLine("Increase command received!");
                commLogic.appLogic.IncreaseBalance(msg.communicationBlock.payload.balance);
                commLogic.IncreaseVectorClock();
                return true;
            }
            else if (msg.controlBlock.Command == ControlCommand.DecreaseBalance)
            {
                Console.WriteLine("Decrease command received!");
                commLogic.appLogic.DecreaseBalance(msg.communicationBlock.payload.balance);
                commLogic.IncreaseVectorClock();

                return true;
            }
            else if (msg.controlBlock.Command == ControlCommand.Echo)
            {
                Console.WriteLine("Echo command received!");
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AnswerHost(Message msg)
        {
            using (UdpClient client = new UdpClient())
            {
                client.Connect(IPAddress.Loopback, 1340);

                byte[] data = MessageSerializer.Serialze(msg);
                client.Send(data, data.Length);

                Console.WriteLine($"Answer sent to {IPAddress.Loopback}:1340");
            }
        }
    }
}
