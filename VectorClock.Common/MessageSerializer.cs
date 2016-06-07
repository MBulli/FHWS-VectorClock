using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorClock.Common
{
    public class MessageSerializer
    {
        public static byte[] Serialze(Message msg)
        {
            using (MemoryStream memStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memStream))
            {
                writer.Write((int)msg.type);

                if (msg.controlBlock != null)
                {
                    WriteControlBlock(writer, msg.controlBlock);
                } else
                {
                    WriteControlBlock(writer, new Message.ControlBlock());
                }

                if (msg.communicationBlock != null)
                {
                    WriteCommunicationBlock(writer, msg.communicationBlock);
                } else
                {
                    WriteCommunicationBlock(writer, new Message.CommunicationBlock());
                }

                return memStream.GetBuffer();
            }
        }

        private static void WriteControlBlock(BinaryWriter writer, Message.ControlBlock block)
        {
            writer.Write((int)block.Command);
        }

        private static void WriteCommunicationBlock(BinaryWriter writer, Message.CommunicationBlock block)
        {
            if(block.payload == null)
            {
                block.payload = new Message.CommunicationPayload();
            }
            writer.Write((int)block.payload.balance);   // write payload

            if(block.clock == null)
            {
                block.clock = new VectorClockImpl(-1);
            }
            SerializeClock(writer, block.clock);        // write clock
        }

        private static void SerializeClock(BinaryWriter writer, VectorClockImpl clock)
        {
            writer.Write((int)clock.getID());
            writer.Write((int)clock[0]);
            writer.Write((int)clock[1]);
            writer.Write((int)clock[2]);
        }
    }
}
