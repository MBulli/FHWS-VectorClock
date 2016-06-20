using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VectorClock.Common
{

    public class CausalBroadcast
    {
        public Message msg;
        int broadcastID;

        public CausalBroadcast(int id, Message message)
        {
            msg = message;
            broadcastID = id;
        }

        public bool isBroadcastReady(int[] receivedIDs)
        {
            return (receivedIDs[0] + 1 == broadcastID 
                && receivedIDs[1] + 1 == broadcastID 
                && receivedIDs[2] + 1 == broadcastID);          // True if all previously sent broadcasts came back!
        }
    }
}
