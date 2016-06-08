using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VectorClock.Common
{
    public class Message
    {
        public MessageType type;
        public IPEndPoint senderAddress;

        public ControlBlock controlBlock;       
        public CommunicationBlock communicationBlock; 


        public Message()
        {
            type = new MessageType();
            senderAddress = new IPEndPoint(IPAddress.Any, 0);

            controlBlock = new ControlBlock();
            controlBlock.Command = new ControlCommand();
            communicationBlock = new CommunicationBlock();
            communicationBlock.clock = new VectorClockImpl(-1);
            communicationBlock.payload = new CommunicationPayload();
        }

        public class ControlBlock
        {
            public ControlCommand Command;
            public decimal BalanceDelta;
        }

        public class CommunicationBlock
        {
            public VectorClockImpl clock;
            public CommunicationPayload payload;
        }
        
        public class CommunicationPayload
        {
            public decimal balance;
            public int port;            // port that a node sends a message to 
        }
        
    }

    public enum MessageType : int
    {
        ControlCommand,
        Communication
    }

    public enum ControlCommand : int
    {
        Shutdown,
        IncreaseBalance,
        DecreaseBalance,
        SendMessageTo,
        Update,
        Echo
    }
}
