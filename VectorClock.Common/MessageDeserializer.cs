using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace VectorClock.Common
{
    public class MessageDeserializer
    {
        public static Message Deserialize(byte[] data)
        {
            using (MemoryStream memStream = new MemoryStream(data))
            {
                BinaryFormatter bf = new BinaryFormatter();
                Message msg = (Message)bf.Deserialize(memStream);
                return msg;
            }          
        }
   }
}
