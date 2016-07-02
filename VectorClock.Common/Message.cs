using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VectorClock.Common
{
    [Serializable]
    public class Message
    {
        public MessageType type;
        public IPEndPoint senderAddress;
        public int casualBroadcastID = 0;

        public ControlBlock controlBlock;       
        public CommunicationBlock communicationBlock; 


        public Message()
        {
            type = new MessageType();

            controlBlock = new ControlBlock();
            controlBlock.Command = new ControlCommand();
            communicationBlock = new CommunicationBlock();
            communicationBlock.clock = new VectorClockImpl(-1);
            communicationBlock.payload = new CommunicationPayload();
        }

        public Message(Message msg)
        {
            type = msg.type;
            controlBlock = new Message.ControlBlock(msg.controlBlock);
            communicationBlock = new Message.CommunicationBlock(msg.communicationBlock);
        }

        [Serializable]
        public class ControlBlock
        {
            public ControlBlock(Message.ControlBlock input)
            {
                Command = input.Command;
                BalanceDelta = input.BalanceDelta;
                SendMessageTarget = new IPEndPoint(input.SendMessageTarget.Address, input.SendMessageTarget.Port);
            }

            public ControlBlock()
            {
                Command = new ControlCommand();
                BalanceDelta = 0;
                SendMessageTarget = new IPEndPoint(IPAddress.None, 0);
            }
            public ControlCommand Command;
            public decimal BalanceDelta; // used by In/Decrease/SetBalance
            public IPEndPoint SendMessageTarget = new IPEndPoint(IPAddress.None, 0); // used by SendMessageTo
        }

        [Serializable]
        public class CommunicationBlock
        {
            public CommunicationBlock(CommunicationBlock input)
            {
                clock = new VectorClockImpl(input.clock);
                payload = new CommunicationPayload(input.payload);
            }

            public CommunicationBlock()
            {
                clock = new VectorClockImpl(-1);
                payload = new CommunicationPayload();
            }

            public VectorClockImpl clock;
            public CommunicationPayload payload;
        }

        [Serializable]
        public class CommunicationPayload
        {
            public CommunicationPayload(CommunicationPayload input)
            {
                balance = input.balance;
            }

            public CommunicationPayload()
            {
                balance = 0;
            }

            public decimal balance;
        }
        
    }

    public enum MessageType : int
    {
        ControlCommand,
        Communication
    }

    public enum ControlCommand : int
    {
        SetBalance,
        UpdateBalance,
        SendMessageTo,
        Updated,
        Echo
    }
}
