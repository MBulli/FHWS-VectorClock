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
        int id = -1;
        int[] timestamps;

        public VectorClockImpl(int ownerIndex)
        {
            this.timestamps = new int[3];
            this.id = ownerIndex;
        }

        public int Length { get { return timestamps.Length; } }
        public int this[int index]
        {
            get { return timestamps[index]; }
            set { timestamps[index] = value; }
        }

        public void Increment()
        {
            timestamps[this.id]++;
        }

        public void Set(int index, int value)
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

        private bool AllElementsLessThanOrEqual(int[] a, int[] b)
        {
            for (int k = 0; k < a.Length; k++)
            {
                if (!(a[k] <= b[k]))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AnyElementLessThan(int[] a, int[] b)
        {
            for (int k = 0; k < a.Length; k++)
            {
                if (a[k] < b[k])
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
