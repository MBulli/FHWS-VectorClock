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
        public IPAddress senderAddress;

        public ControlBlock controlBlock; // Null if type != ControlCommand       
        public CommunicationBlock communicationBlock; // Null if type != Communication

        public class ControlBlock
        {
            public ControlCommand Command;
        }

        public class CommunicationBlock
        {
            public VectorClockImpl clock;
            public CommunicationPayload payload;

        }
        
        public class CommunicationPayload
        {
            public decimal balance;
        }
        
    }

    public enum MessageType
    {
        ControlCommand,
        Communication
    }

    public enum ControlCommand
    {
        Shutdown,
        IncreaseBalance,
        DecreaseBalance,
        Echo
    }
}
