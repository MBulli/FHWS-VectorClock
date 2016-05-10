using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VectorClock.Common;

namespace VectorClock.Node
{
    class ControlLogic
    {
        CommunicationLogic commLogic;

        public ControlLogic(CommunicationLogic commLogic)
        {
            this.commLogic = commLogic;
        }

        public bool HandleMessage(Message msg)
        {
            if (msg.controlBlock.Command == ControlCommand.Shutdown)
            {
                Console.WriteLine("Shutdown command received!");
                return true;
            }
            else if (msg.controlBlock.Command == ControlCommand.IncreaseBalance)
            {
                Console.WriteLine("Increase command received!");
                commLogic.appLogic.IncreaseBalance(msg.communicationBlock.payload.balance);
                commLogic.IncreaseVectorClock();
                return true;
            }
            else if (msg.controlBlock.Command == ControlCommand.DecreaseBalance)
            {
                Console.WriteLine("Decrease command received!");
                commLogic.appLogic.DecreaseBalance(msg.communicationBlock.payload.balance);
                commLogic.IncreaseVectorClock();

                return true;
            }
            else if (msg.controlBlock.Command == ControlCommand.Echo)
            {
                Console.WriteLine("Echo command received!");
                return true;
            }
            else
            {
                return false;
            }
        }
        
    }
}
