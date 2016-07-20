using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VectorClock.Common;

namespace VectorClock.Node
{
    class ControlLogic
    {
        CommunicationLogic commLogic;
        IPEndPoint localEndpoint;
        OrderedMessageList delayedMessages;
        Random random;

        public ControlLogic(CommunicationLogic commLogic, IPEndPoint endPoint)
        {
            this.commLogic = commLogic;
            this.localEndpoint = endPoint;
            this.delayedMessages = new OrderedMessageList();
            random = new Random(0);
        }

        public bool HandleMessage(bool causallyOrdered, Message msg, IPEndPoint remoteEP)
        {
            Console.WriteLine("-------------------------------------");
            bool returnValue = false;

            if (msg.type == MessageType.ControlCommand)
            {
                if (causallyOrdered)
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
                if (causallyOrdered)
                {
                    HandleCommunicationMessageOrdered(ref msg);
                }
                else
                {
                    HandleCommunicationMessage(ref msg);
                }
            }

            // Answer host
            Message hostAnswer = new Message(msg);
            hostAnswer.communicationBlock.clock = new VectorClockImpl(this.commLogic.clock);
            hostAnswer.communicationBlock.payload.balance = this.commLogic.appLogic.balance;
            ReportToCommander(hostAnswer);

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
            else if (msg.controlBlock.Command == ControlCommand.UpdateBalance)
            {
                Console.WriteLine("Increase command received!");
                commLogic.appLogic.UpdateBalance(msg.controlBlock.BalanceDelta);
                Console.WriteLine($"New balance: {commLogic.appLogic.balance}");
                commLogic.IncreaseVectorClock();                                    // Increase before broadcast if unordered
                Console.WriteLine($"New Clock: {this.commLogic.clock}");
                BroadcastChange(msg);
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
            else if (msg.controlBlock.Command == ControlCommand.UpdateBalance)
            {
                Console.WriteLine("Increase command received!");
                commLogic.appLogic.UpdateBalance(msg.controlBlock.BalanceDelta);
                Console.WriteLine($"New balance: {commLogic.appLogic.balance}");
                BroadcastChange(msg);
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
            this.commLogic.appLogic.balance += msg.communicationBlock.payload.balance;
            this.commLogic.IncreaseVectorClock();
            Console.WriteLine($"    Increased own clock: {this.commLogic.clock}");
            msg.controlBlock.Command = ControlCommand.Updated;

            return returnValue;
        }

        private void HandleCommunicationMessageOrdered(ref Message msg)
        {
            bool allreadyPushed = false;

            Console.WriteLine("Update command received!");

            Console.WriteLine($"    Update: Own clock: {this.commLogic.clock} Other clock: {msg.communicationBlock.clock}");
            Console.WriteLine($"    Update: Old balance: {this.commLogic.appLogic.balance} New Balance: {msg.communicationBlock.payload.balance}");

            // check if message is suitable

            if (delayedMessages.Count == 0) // no delayed messages
            {
                if (!MessageAcceptable(msg))   //  delay until vc(x) <= m.vc(x) for all x
                {
                    delayedMessages.PushItem(msg);   // Put message in queue
                    Console.WriteLine($"    Message {msg.communicationBlock.clock} is to early, delaying...");
                }
                else
                    UseMessageOrdered(msg);     // Message ist ok, use it!
            }
            else
            {
                if (!MessageAcceptable(msg))
                {
                    for (int i = 0; i < delayedMessages.Count; i++)
                    {
                        Message current = delayedMessages.PopItem();
                        if (MessageAcceptable(current))
                        {
                            UseMessageOrdered(current);
                            if (!MessageAcceptable(msg))
                            {
                                delayedMessages.PushItem(msg);
                                allreadyPushed = true;
                                Debug.WriteLine("Multiple messages delayed!!!");
                                Console.WriteLine("Multiple messages delayed!!!");
                                break;
                            }
                            else
                            {
                                UseMessageOrdered(msg);
                                allreadyPushed = true;
                                TestDelayedMessages();
                                break;
                            }
                        }
                        else
                        {
                            delayedMessages.PushItem(current);
                            Debug.WriteLine("Reinserted delayed Message!");
                            Console.WriteLine("Reinserted delayed Message!");
                        }
                    }

                    if (!allreadyPushed)
                    {
                        // Nothing matched, just push msg in delayed list
                        delayedMessages.PushItem(msg);
                    }
                }
                else
                {
                    UseMessageOrdered(msg);
                    TestDelayedMessages();
                }
            }

            msg.controlBlock.Command = ControlCommand.Updated;
        }

        private void UseMessageOrdered(Message msg)
        {
            Console.WriteLine($"    Message {msg.communicationBlock.clock} is ok! Using it.");

            //this.commLogic.clock.update(msg.communicationBlock.clock);
            this.commLogic.appLogic.balance += msg.communicationBlock.payload.balance;
            Console.WriteLine($" Balance in Msg: {msg.communicationBlock.payload.balance}");
            if (this.commLogic.clock.getID() != msg.communicationBlock.clock.getID())
            {
                this.commLogic.IncreaseVectorClock(msg.communicationBlock.clock.getID());           // update control variable
                Console.WriteLine($"    Increased clock[{msg.communicationBlock.clock.getID()}]: {this.commLogic.clock}");
            }

            PrintCurrentValues();
        }

        private void TestDelayedMessages()
        {
            for (int i = 0; i < delayedMessages.Count; i++)
            {
                Message messageToProof = delayedMessages.PopItem();
                if (MessageAcceptable(messageToProof))
                    UseMessageOrdered(messageToProof);
                else
                    delayedMessages.PushItem(messageToProof);
            }
        }

        private bool MessageAcceptable(Message msg)
        {
            return (msg.communicationBlock.clock.Compare(this.commLogic.clock) == ComparisonResult.Equal
                    || msg.communicationBlock.clock.Compare(this.commLogic.clock) == ComparisonResult.Before);
        }

        private void ReportToCommander(Message msg)
        {
            SendMessageTo("HostAnswer", msg, NetworkConfig.CommanderEndpoint);
        }

        private void BroadcastChange(Message handledMessage)
        {
            //var msg = MessageFactory.Communication.CreateUpdateMessage(this.endPoint, this.commLogic.appLogic.balance, this.commLogic.clock); // User this for absolute values
            var msg = MessageFactory.Communication.CreateUpdateMessage(handledMessage.controlBlock.BalanceDelta, this.commLogic.clock);

            var endpoints = NetworkConfig.NodeEndpoints;

            int i = 1;
            foreach (var node in endpoints.Where(ep => !ep.Equals(localEndpoint)))
            {
                if (i == 2)
                {
                    int delayInMilliseconds = random.Next(100, 1000);
                    Console.WriteLine($"Delaying broadcast to node {i} by {delayInMilliseconds} milliseconds.");
                    Thread.Sleep(delayInMilliseconds);
                
                    SendMessageTo("Broadcast", msg, node);
                }
                else
                {
                    SendMessageTo("Broadcast", msg, node);
                }
                //SendMessageTo("Broadcast", msg, node);
                i++;
            }
        }

        private void SendMessageTo(String messageText, Message msg, IPEndPoint node)
        {
            if (this.localEndpoint.Equals(node))
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
        private void PrintCurrentValues()
        {
            // Write all new infos after message use
            Console.WriteLine("New Values:");
            Console.WriteLine($"     Clock: {this.commLogic.clock}");
            Console.WriteLine($"     Balance: {this.commLogic.appLogic.balance}");
        }
    }
}
