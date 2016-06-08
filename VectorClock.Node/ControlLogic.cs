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

            if (msg.controlBlock.Command == ControlCommand.Shutdown)
            {
                Console.WriteLine("Shutdown command received!");
                returnValue = true;
            }
            else if(msg.controlBlock.Command == ControlCommand.SendMessageTo)
            {
                Console.WriteLine($"SendMessageTo({msg.communicationBlock.payload.port}) command received!");
                Message messageToNote = new Message();
                messageToNote.controlBlock.Command = ControlCommand.Update;
                messageToNote.communicationBlock.clock = this.commLogic.clock;
                messageToNote.communicationBlock.payload.balance = this.commLogic.appLogic.balance;

                SendMessageTo(messageToNote.controlBlock.Command.ToString(), messageToNote, msg.communicationBlock.payload.port);
                returnValue = true;
            }
            else if(msg.controlBlock.Command == ControlCommand.Update)
            {
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
                        if(this.commLogic.appLogic.balance != msg.communicationBlock.payload.balance)
                        {
                            Console.WriteLine($" Update: Error, balances are unequal! {this.commLogic.appLogic.balance} != {msg.communicationBlock.payload.balance}");
                        }
                        break;
                    case ComparisonResult.After:
                        Console.WriteLine(" Update: Own clock is newer!");
                        // TODO: what to do here?!
                        break;
                }

            }
            else if (msg.controlBlock.Command == ControlCommand.IncreaseBalance)
            {
                Console.WriteLine("Increase command received!");
                commLogic.appLogic.IncreaseBalance(msg.communicationBlock.payload.balance);
                commLogic.IncreaseVectorClock();
                returnValue = true;
            }
            else if (msg.controlBlock.Command == ControlCommand.DecreaseBalance)
            {
                Console.WriteLine("Decrease command received!");
                commLogic.appLogic.DecreaseBalance(msg.communicationBlock.payload.balance);
                commLogic.IncreaseVectorClock();
                returnValue = true;
            }
            else if (msg.controlBlock.Command == ControlCommand.Echo)
            {
                Console.WriteLine("Echo command received!");
                returnValue = true;
            }

            //msg.communicationBlock = new Message.CommunicationBlock(); // CommBlock from commander-message was empty
            msg.communicationBlock.clock = this.commLogic.clock;
            msg.communicationBlock.payload.balance = this.commLogic.appLogic.balance;
            AnswerHost(msg);

            return returnValue;
        }

        private void AnswerHost(Message msg)
        {
            SendMessageTo("HostAnswer", msg, 1340);
        }

        private void SendMessageTo(String messageText, Message msg, int port)
        {
            using (UdpClient client = new UdpClient())
            {
                client.Connect(IPAddress.Loopback, port);

                byte[] data = MessageSerializer.Serialze(msg);
                client.Send(data, data.Length);

                Console.WriteLine($"Message ({messageText}) sent to {IPAddress.Loopback}:{port}");
            }
        }
    }
}
