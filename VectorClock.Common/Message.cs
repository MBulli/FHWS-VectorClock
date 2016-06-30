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

        [Serializable]
        public class ControlBlock
        {
            public ControlCommand Command;
            public decimal BalanceDelta; // used by In/Decrease/SetBalance
            public IPEndPoint SendMessageTarget = new IPEndPoint(IPAddress.None, 0); // used by SendMessageTo
        }

        [Serializable]
        public class CommunicationBlock
        {
            public VectorClockImpl clock;
            public CommunicationPayload payload;
        }

        [Serializable]
        public class CommunicationPayload
        {
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
