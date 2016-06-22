using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VectorClock.Common
{
    [Serializable]
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

        public int getID()
        {
            return this.id;
        }

        public void Increment()
        {
            timestamps[this.id]++;
        }

        public void Increment(int i)
        {
            timestamps[i]++;
        }

        public void Set(int index, int value)
        {
            timestamps[index] = value;
        }

        public void update(VectorClockImpl other)
        {
            this.timestamps = other.timestamps;
        }

        public ComparisonResult Compare(VectorClockImpl other)
        {
            if(IsEqual(this.timestamps, other.timestamps))
            {
                return ComparisonResult.Equal;
            }
            else if(IsConcurrent(this.timestamps, other.timestamps))
            {
                return ComparisonResult.Concurrent;
            }
            else if(LessThanOrEqual(this.timestamps, other.timestamps))
            {
                return ComparisonResult.Before;
            }

            return ComparisonResult.After;
        }

        private bool LessThanOrEqual(int[] a, int[] b)
        {
            bool lessThanOrEqual = true;

            for(int k = 0; k < a.Length; k++)
            { 
                if (a[k] > b[k])
                {
                    lessThanOrEqual = false;
                }
            }

            return lessThanOrEqual;
        }

        private bool IsConcurrent(int[] a, int[] b)
        {
            bool greater = false;
            bool less = false;

            for(int i = 0; i < a.Length; i++)
            {
                if (a[i] > b[i])
                {
                    greater = true;
                }
                else if (a[i] < b[i])
                {
                    less = true;
                }           
            }

            return (greater && less);
        }

        private bool IsEqual(int[] a, int[] b)
        {
            bool isEqual = true;

            for(int k = 0; k < a.Length; k++)
            {
                if (a[k] != b[k])
                {
                    isEqual = false;
                }
            }

            return isEqual;
        }
        

        public override string ToString()
        {
            return $"ID= {id}: [{string.Join(",", timestamps)}]";
        }
    }

    public enum ComparisonResult
    {
        Before,
        After,
        Concurrent,
        Equal
    }
}
