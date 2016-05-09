using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorClock.Common
{
    public class MessageDeserializer
    {
        public static Message Deserialize(byte[] data)
        {
            using (MemoryStream memStream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(memStream))
            {
                Message msg = new Message();
                msg.type = (MessageType)reader.ReadInt32();

                if (msg.type == MessageType.ControlCommand)
                {
                    msg.controlBlock = ReadControlBlock(reader);
                }
                else
                {
                    msg.communicationBlock = ReadCommunicationBlock(reader);
                }

                return msg;
            }          
        }

        public static Message.ControlBlock ReadControlBlock(BinaryReader reader)
        {
            ControlCommand cmd = (ControlCommand)reader.ReadInt32();

            return new Message.ControlBlock()
            {
                Command = cmd
            };
        }

        public static Message.CommunicationBlock ReadCommunicationBlock(BinaryReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
