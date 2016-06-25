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
        IPEndPoint endPoint;
        OrderedMessageList delayedMessages;

        public ControlLogic(CommunicationLogic commLogic, IPEndPoint endPoint)
        {
            this.commLogic = commLogic;
            this.endPoint = endPoint;
            this.delayedMessages = new OrderedMessageList();
        }

        public bool HandleMessage(bool causallyOrdered, Message msg, IPEndPoint remoteEP)
        {
            bool returnValue = false;

            if(msg.type == MessageType.ControlCommand)
            {
                if(causallyOrdered)
                {
                    HandleControlMessageOrdered(ref msg);
                }
                else
                {
                    HandleControlMessage(ref msg);
                }
            }
            else
            { 
                if(causallyOrdered)
                {
                    HandleCommunicationMessageOrdered(ref msg);
                }
                else
                {
                    HandleCommunicationMessage(ref msg);
                }
            }

            // Answer host
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
                commLogic.IncreaseVectorClock();                                    // Increase before broadcast if unordered
                Console.WriteLine($"New Clock: {this.commLogic.clock}");
                BroadcastChange();
                returnValue = true;
            }
            else if (msg.controlBlock.Command == ControlCommand.DecreaseBalance)
            {
                Console.WriteLine("Decrease command received!");
                commLogic.appLogic.DecreaseBalance(msg.controlBlock.BalanceDelta);
                Console.WriteLine($"New balance: {commLogic.appLogic.balance}");
                commLogic.IncreaseVectorClock();                                    // Increase before broadcast if unordered
                Console.WriteLine($"New Clock: {this.commLogic.clock}");
                BroadcastChange();
                returnValue = true;
            }
            else if (msg.controlBlock.Command == ControlCommand.Echo)
            {
                Console.WriteLine("Echo command received!");
                returnValue = true;
            }

            return returnValue;
        }

        private bool HandleControlMessageOrdered(ref Message msg)
        {
            bool returnValue = false;

           
            if (msg.controlBlock.Command == ControlCommand.SetBalance)
            {
                Console.WriteLine("Set balance command received!");
                commLogic.appLogic.balance = msg.controlBlock.BalanceDelta;
                Console.WriteLine($"New balance: {commLogic.appLogic.balance}");
                
                returnValue = true;
            }
            else if (msg.controlBlock.Command == ControlCommand.IncreaseBalance)
            {
                Console.WriteLine("Increase command received!");
                commLogic.appLogic.IncreaseBalance(msg.controlBlock.BalanceDelta);
                Console.WriteLine($"New balance: {commLogic.appLogic.balance}");
                BroadcastChange();
                commLogic.IncreaseVectorClock(); // Increase after broadcast if ordered
                Console.WriteLine($"New Clock: {this.commLogic.clock}");
                returnValue = true;
            }
            else if (msg.controlBlock.Command == ControlCommand.DecreaseBalance)
            {
                Console.WriteLine("Decrease command received!");
                commLogic.appLogic.DecreaseBalance(msg.controlBlock.BalanceDelta);
                Console.WriteLine($"New balance: {commLogic.appLogic.balance}");
                BroadcastChange();
                commLogic.IncreaseVectorClock(); // Increase after broadcast if ordered
                Console.WriteLine($"New Clock: {this.commLogic.clock}");
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
            Console.WriteLine("Update command received!");

            Console.WriteLine($"    Update: Own clock: {this.commLogic.clock} Other clock: {msg.communicationBlock.clock}");
            Console.WriteLine($"    Update: Old balance: {this.commLogic.appLogic.balance} New Balance: {msg.communicationBlock.payload.balance}");

            this.commLogic.clock.update(msg.communicationBlock.clock);
            this.commLogic.appLogic.balance = msg.communicationBlock.payload.balance;
            this.commLogic.IncreaseVectorClock();
            Console.WriteLine($"    Increased own clock: {this.commLogic.clock}");
            msg.controlBlock.Command = ControlCommand.Updated;
            
            return returnValue;
        }

        private void HandleCommunicationMessageOrdered(ref Message msg)
        {
            Console.WriteLine("Update command received!");

            Console.WriteLine($"    Update: Own clock: {this.commLogic.clock} Other clock: {msg.communicationBlock.clock}");
            Console.WriteLine($"    Update: Old balance: {this.commLogic.appLogic.balance} New Balance: {msg.communicationBlock.payload.balance}");

            // check if message is suitable

            if (delayedMessages.Count == 0) // no delayed messages
            {
                if (MessageNotAcceptable(msg))   //  delay until vc(x) <= m.vc(x) for all x
                {
                    delayedMessages.PushItem(msg);   // Put message in queue
                    Console.WriteLine($"    Message {msg.communicationBlock.clock} is to early, delaying...");
                }
                else
                    UseMessageOrdered(msg);     // Message ist ok, use it!
            }
            else
            {
                if(MessageNotAcceptable(msg))
                {
                    for(int i = 0; i < delayedMessages.Count; i ++)
                    {
                        Message current = delayedMessages.PopItem();
                        if(!MessageNotAcceptable(current))
                        {
                            delayedMessages.PopItem();
                            UseMessageOrdered(current);
                        }
                    }
                    if(MessageNotAcceptable(msg))
                        delayedMessages.PushItem(msg);
                }
                else
                {
                    UseMessageOrdered(msg);
                    Message messageToProof = delayedMessages.PopItem();
                    if (!MessageNotAcceptable(messageToProof))
                        UseMessageOrdered(messageToProof);
                }
            }

            msg.controlBlock.Command = ControlCommand.Updated;
        }

        private void UseMessageOrdered(Message msg)
        {
            Console.WriteLine($"    Message {msg.communicationBlock.clock} is ok! Using it.");
            this.commLogic.clock.update(msg.communicationBlock.clock);
            this.commLogic.appLogic.balance = msg.communicationBlock.payload.balance;
            if (this.commLogic.clock.getID() != msg.communicationBlock.clock.getID())
            {
                this.commLogic.IncreaseVectorClock(msg.communicationBlock.clock.getID());           // update control variable
                Console.WriteLine($"    Increased clock[{msg.communicationBlock.clock.getID()}]: {this.commLogic.clock}");
            }
        }

        private bool MessageNotAcceptable(Message msg)
        {
            return (msg.communicationBlock.clock.Compare(this.commLogic.clock) == ComparisonResult.After);
        }

        private void AnswerHost(Message msg)
        {
            SendMessageTo("HostAnswer", msg, new IPEndPoint(IPAddress.Loopback, 1340));
        }

        private void BroadcastChange()
        {
            var msg = MessageFactory.Communication.CreateUpdateMessage(this.endPoint, this.commLogic.appLogic.balance, this.commLogic.clock);

            var endpoints = new IPEndPoint[] {
                new IPEndPoint(IPAddress.Loopback, 1337),
                new IPEndPoint(IPAddress.Loopback, 1338),
                new IPEndPoint(IPAddress.Loopback, 1339)
            };

            foreach (var node in endpoints.Where(ep => !ep.Equals(endPoint)))
            {
                SendMessageTo("Broadcast", msg, node);
            }
        }

        private void SendMessageTo(String messageText, Message msg, IPEndPoint node)
        {
            if(this.endPoint.Equals(node))
            {
                throw new InvalidOperationException("Sendmessage: Endpoints are equal!");
            }
            using (UdpClient client = new UdpClient())
            {
                client.Connect(node);

                byte[] data = MessageSerializer.Serialze(msg);
                client.Send(data, data.Length);

                Console.WriteLine($"Message ({messageText}) sent to {node}");
            }
        }
    }
}
