using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VectorClock.Common
{
    public class MessageFactory
    {
        public static class Control
        {
        }

        public static class Communication
        {
            public static Message CreateUpdateMessage(IPEndPoint senderAdress, decimal balance, VectorClockImpl clock)
            {
                Message msg = new Message();
                msg.type = MessageType.Communication;
                msg.communicationBlock.clock = clock;
                msg.communicationBlock.payload.balance = balance;
                msg.senderAddress = senderAdress;
                return msg;
            }

            public static Message CreateCausalBroadcastMessage(IPEndPoint senderAddress, decimal balance, VectorClockImpl clock, int broadcastID)
            {
                Message msg = new Message();
                msg.type = MessageType.Communication;
                msg.controlBlock.Command = ControlCommand.AnswerToBroadcast;
                msg.communicationBlock.clock = clock;
                msg.communicationBlock.payload.balance = balance;
                msg.casualBroadcastID = broadcastID;
                msg.senderAddress = senderAddress;
                return msg;
            }

            public static Message CreateCausalBroadcastAnswerMessage(IPEndPoint senderAddress, int nodeID, int broadcastID)
            {
                Message msg = new Message();
                msg.type = MessageType.Communication;
                msg.controlBlock.Command = ControlCommand.BroadcastAnswer;
                msg.communicationBlock.clock = new VectorClockImpl(nodeID);
                msg.casualBroadcastID = broadcastID;
                msg.senderAddress = senderAddress;
                return msg;
            }
        }
    }
}
