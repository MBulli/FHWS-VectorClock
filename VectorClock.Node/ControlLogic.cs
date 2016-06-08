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
            bool returnValue = false;

            if(msg.type == MessageType.ControlCommand)
            {
                HandleControlMessage(ref msg);
            }
            else
            {
                HandleCommunicationMessage(ref msg);
            }

            msg.communicationBlock.clock = this.commLogic.clock;
            msg.communicationBlock.payload.balance = this.commLogic.appLogic.balance;
            AnswerHost(msg);

            return returnValue;
        }

        private bool HandleControlMessage(ref Message msg)
        {
            bool returnValue = false;

            if (msg.controlBlock.Command == ControlCommand.SendMessageTo)
            {
                Console.WriteLine($"SendMessageTo({msg.controlBlock.SendMessageTarget}) command received!");

                Message messageToNote = new Message();
                messageToNote.type = MessageType.Communication;
                messageToNote.communicationBlock.clock = this.commLogic.clock;
                messageToNote.communicationBlock.payload.balance = this.commLogic.appLogic.balance;

                SendMessageTo(messageToNote.controlBlock.Command.ToString(), messageToNote, msg.controlBlock.SendMessageTarget);
                returnValue = true;
            }
            else if (msg.controlBlock.Command == ControlCommand.SetBalance)
            {
                Console.WriteLine("Set balance command received!");
                commLogic.appLogic.balance = msg.controlBlock.BalanceDelta;
                Console.WriteLine($"New balance: {commLogic.appLogic.balance}");
                commLogic.IncreaseVectorClock();
                returnValue = true;
            }
            else if (msg.controlBlock.Command == ControlCommand.IncreaseBalance)
            {
                Console.WriteLine("Increase command received!");
                commLogic.appLogic.IncreaseBalance(msg.controlBlock.BalanceDelta);
                Console.WriteLine($"New balance: {commLogic.appLogic.balance}");
                commLogic.IncreaseVectorClock();
                returnValue = true;
            }
            else if (msg.controlBlock.Command == ControlCommand.DecreaseBalance)
            {
                Console.WriteLine("Decrease command received!");
                commLogic.appLogic.DecreaseBalance(msg.controlBlock.BalanceDelta);
                Console.WriteLine($"New balance: {commLogic.appLogic.balance}");
                commLogic.IncreaseVectorClock();
                returnValue = true;
            }
            else if (msg.controlBlock.Command == ControlCommand.Echo)
            {
                Console.WriteLine("Echo command received!");
                returnValue = true;
            }

            return returnValue;
        }
        private bool HandleCommunicationMessage(ref Message msg)
        {
            bool returnValue = false;

            msg.controlBlock.Command = ControlCommand.Updated;

            returnValue = true;
            Console.WriteLine("Update command received!");
            // TODO: Check if the clock from received updateMessage is newer than own and handle that!

            switch (this.commLogic.clock.Compare(msg.communicationBlock.clock))
            {
                case ComparisonResult.Before:
                    Console.WriteLine(" Update: Own clock is older, update!");
                    Console.WriteLine($" Update: Old clock: {this.commLogic.clock} New clock: {msg.communicationBlock.clock}");
                    Console.WriteLine($" Update: Old balance: {this.commLogic.appLogic.balance} New Balance: {msg.communicationBlock.payload.balance}");

                    this.commLogic.clock.update(msg.communicationBlock.clock);
                    this.commLogic.appLogic.balance = msg.communicationBlock.payload.balance;
                    break;
                case ComparisonResult.Concurrent:
                    Console.WriteLine(" Update: Clocks are equal!");
                    if (this.commLogic.appLogic.balance != msg.communicationBlock.payload.balance)
                    {
                        Console.WriteLine($" Update: Error, balances are unequal! {this.commLogic.appLogic.balance} != {msg.communicationBlock.payload.balance}");
                    }
                    break;
                case ComparisonResult.After:
                    Console.WriteLine(" Update: Own clock is newer!");
                    // TODO: what to do here?!
                    break;
            }
           
            return returnValue;
        }

        private void AnswerHost(Message msg)
        {
            SendMessageTo("HostAnswer", msg, new IPEndPoint(IPAddress.Loopback, 1340));
        }

        private void SendMessageTo(String messageText, Message msg, IPEndPoint endpoint)
        {
            using (UdpClient client = new UdpClient())
            {
                client.Connect(endpoint);

                byte[] data = MessageSerializer.Serialze(msg);
                client.Send(data, data.Length);

                Console.WriteLine($"Message ({messageText}) sent to {endpoint}");
            }
        }
    }
}
