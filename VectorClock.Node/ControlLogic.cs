﻿using System;
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

        public ControlLogic(CommunicationLogic commLogic, IPEndPoint endPoint)
        {
            this.commLogic = commLogic;
            this.endPoint = endPoint;
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
                Console.WriteLine($"New Clock: {this.commLogic.clock}");
                BroadcastChange();
                returnValue = true;
            }
            else if (msg.controlBlock.Command == ControlCommand.DecreaseBalance)
            {
                Console.WriteLine("Decrease command received!");
                commLogic.appLogic.DecreaseBalance(msg.controlBlock.BalanceDelta);
                Console.WriteLine($"New balance: {commLogic.appLogic.balance}");
                commLogic.IncreaseVectorClock();
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
        private bool HandleCommunicationMessage(ref Message msg)
        {
            bool returnValue = false;

            msg.controlBlock.Command = ControlCommand.Updated;

            returnValue = true;
            Console.WriteLine("Update command received!");

            updateUninitializedClockValues(msg.communicationBlock.clock);

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
                    Console.WriteLine(" Update: Clocks are concurrent!");
                    Console.WriteLine($" Update: Own clock: {this.commLogic.clock} Other clock: {msg.communicationBlock.clock}");
                    if (this.commLogic.appLogic.balance != msg.communicationBlock.payload.balance)
                    {
                        Console.WriteLine($" Update: Error, balances are unequal! {this.commLogic.appLogic.balance} != {msg.communicationBlock.payload.balance}");
                    }
                    break;
                case ComparisonResult.After:
                    Console.WriteLine(" Update: Own clock is newer! Merge!");
                    Console.WriteLine($" Update: Old clock: {this.commLogic.clock} New clock: {msg.communicationBlock.clock}");
                    Console.WriteLine($" Update: Old balance: {this.commLogic.appLogic.balance} New Balance: {msg.communicationBlock.payload.balance}");

                    this.commLogic.clock.update(msg.communicationBlock.clock);
                    this.commLogic.appLogic.balance = msg.communicationBlock.payload.balance;
                    
                    break;
            }
            this.commLogic.IncreaseVectorClock();
            Console.WriteLine($"Increased own clock: {this.commLogic.clock}");


            return returnValue;
        }

        private void AnswerHost(Message msg)
        {
            SendMessageTo("HostAnswer", msg, new IPEndPoint(IPAddress.Loopback, 1340));
        }

        private void BroadcastChange()
        {
            var msg = MessageFactory.Communication.CreateUpdateMessage(this.commLogic.appLogic.balance, this.commLogic.clock);

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

        private void updateUninitializedClockValues(VectorClockImpl otherClock)
        {
            for(int i = 0; i < otherClock.Length; i++)
            {
                if(this.commLogic.clock[i] == 0)
                {
                    this.commLogic.clock[i] = otherClock[i];
                }
            }
        }
    }
}
