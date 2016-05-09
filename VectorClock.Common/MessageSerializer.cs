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

                if (msg.type == MessageType.ControlCommand)
                {
                    WriteControlBlock(writer, msg.controlBlock);
                }
                else
                {
                    WriteCommunicationBlock(writer, msg.communicationBlock);
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

        }
    }
}
