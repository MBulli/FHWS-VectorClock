using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace VectorClock.Common
{
    public class MessageSerializer
    {
        public static byte[] Serialze(Message msg)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(memStream, msg);
                return memStream.GetBuffer();
            }
        }
    }
}
