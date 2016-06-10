using System;
using System.Collections.Generic;
using System.Linq;
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
            public static Message CreateUpdateMessage(decimal balance, VectorClockImpl clock)
            {
                Message msg = new Message();
                msg.type = MessageType.Communication;
                msg.communicationBlock.clock = clock;
                msg.communicationBlock.payload.balance = balance;
                return msg;
            }
        }
    }
}
