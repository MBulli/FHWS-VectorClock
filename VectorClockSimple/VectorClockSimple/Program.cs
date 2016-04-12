using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var A = new Process("A", 0);
            var B = new Process("B", 1);
            var C = new Process("C", 2);

            A.SendMessageTo(B, 50);
            C.SendMessageTo(B);

            Console.ReadLine();
            return;
        }
    }

    class Process
    {
        string name = "";
        int index = 0;
        VectorClock clock;

        public Process(string name, int index)
        {
            this.name = name;
            this.index = index;
            this.clock = new VectorClock(index);
        }

        public async void SendMessageTo(Process target, int delay = 0)
        {
            Console.WriteLine($"{name} sends message to {target.name}.");

            clock.Increment(this.index);

            await target.Receive(this, new Message(clock), delay);
        }

        public async Task Receive(Process source, Message m, int delayInMs)
        {
            await Task.Delay(delayInMs);

            Console.WriteLine($"{name} received message from {source.name} with m.VC={m.VC}.");

            clock.Increment(this.index);

            for (int x = 0; x < m.VC.Length; x++)
            {
                this.clock[x] = Math.Max(this.clock[x], m.VC[x]);
            }

            Console.WriteLine($"New clock of {name}={clock}.");
        }
    }

    enum ComparisonResult
    {
        Before,
        After,
        Concurrent
    }

    struct VectorClock
    {
        int ownerIndex;
        int[] timestamps;

        public VectorClock(int ownerIndex)
        {
            this.timestamps = new int[3];
            this.ownerIndex = ownerIndex;
        }

        public int Length { get { return timestamps.Length; } }
        public int this[int index]
        {
            get { return timestamps[index]; }
            set { timestamps[index] = value; }
        }

        public void Increment(int index)
        {
            timestamps[index]++;
        }

        public void Set(int index, int value)
        {
            timestamps[index] = value;
        }

        public ComparisonResult Compare(VectorClock other)
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

    struct Message
    {
        public readonly VectorClock VC;

        public Message(VectorClock vc)
        {
            this.VC = vc;
        }
    }
}
