using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VectorClock.Common
{
    public class VectorClockImpl
    {
        IPAddress ownerAddress;
        Dictionary<IPAddress, int> timestamps;

        public VectorClockImpl(IPAddress ownerAddress)
        {
            this.timestamps = new Dictionary<IPAddress, int>();
            this.ownerAddress = ownerAddress;
        }

        public int Length { get { return timestamps.Count; } }
        public int this[IPAddress index]
        {
            get { return timestamps[index]; }
            set { timestamps[index] = value; }
        }

        public void Increment(IPAddress index)
        {
            timestamps[index]++;
        }

        public void Set(IPAddress index, int value)
        {
            timestamps[index] = value;
        }

        public ComparisonResult Compare(VectorClockImpl other)
        {
            // Check this before other
            if (AllElementsLessThanOrEqual(this.timestamps, other.timestamps)
             && AnyElementLessThan(this.timestamps, other.timestamps))
            {
                return ComparisonResult.Before;
            }


            throw new NotImplementedException();
        }

        private bool AllElementsLessThanOrEqual(Dictionary<IPAddress, int> a, Dictionary<IPAddress, int> b)
        {
            for (int k = 0; k < a.Count; k++)
            {
                if (!(a.ElementAt(k).Value <= b.ElementAt(k).Value))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AnyElementLessThan(Dictionary<IPAddress, int> a, Dictionary<IPAddress, int> b)
        {
            for (int k = 0; k < a.Count; k++)
            {
                if (a.ElementAt(k).Value < b.ElementAt(k).Value)
                {
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return $"[{string.Join(",", timestamps)}]";
        }
    }

    public enum ComparisonResult
    {
        Before,
        After,
        Concurrent
    }
}
