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

                msg.controlBlock = ReadControlBlock(reader);
                msg.communicationBlock = ReadCommunicationBlock(reader);
                
                return msg;
            }          
        }

        public static Message.ControlBlock ReadControlBlock(BinaryReader reader)
        {
            ControlCommand cmd = (ControlCommand)reader.ReadInt32();
            decimal balanceDelta = reader.ReadDecimal();

            return new Message.ControlBlock()
            {
                Command = cmd,
                BalanceDelta = balanceDelta
            };
        }

        public static Message.CommunicationBlock ReadCommunicationBlock(BinaryReader reader)
        {
            int balance = (int)reader.ReadInt32();
            int port = (int)reader.ReadInt32();

            Message.CommunicationBlock block = new Message.CommunicationBlock();
            block.payload = new Message.CommunicationPayload();
            block.payload.balance = balance;
            block.payload.port = port;

            VectorClockImpl clock = new VectorClockImpl((int)reader.ReadInt32());

            clock[0] = (int)reader.ReadInt32();
            clock[1] = (int)reader.ReadInt32();
            clock[2] = (int)reader.ReadInt32();

            block.clock = clock;

            return block;
        }
    }
}
