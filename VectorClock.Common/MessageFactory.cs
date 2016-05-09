using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorClock.Common
{
    public class MessageFactory
    {
        public class Control
        {
            public static Message Shutdown()
            {
                return new Message()
                {
                    type = MessageType.ControlCommand,
                    controlBlock = new Message.ControlBlock()
                    {
                        Command = ControlCommand.Shutdown
                    }
                };
            }
        }
    }
}
