using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VectorClock.Common;

namespace VectorClock.Node
{
    class CommunicationLogic
    {
        public readonly VectorClockImpl clock;

        public readonly ApplicationLogic appLogic;

        public CommunicationLogic(ApplicationLogic appLogic, int ownerIndex)
        {
            this.clock = new VectorClockImpl(ownerIndex);
            this.appLogic = appLogic;
        }

        public void IncreaseVectorClock()
        {
            clock.Increment();
        }

        public void UpdateClock(VectorClockImpl newerClock)
        {
            for(int i = 0; i < newerClock.Length; i++)
            {
                this.clock.Set(i, newerClock[i]);
            }
        }
    }
}
