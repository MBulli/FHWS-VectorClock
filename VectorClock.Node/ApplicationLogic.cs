using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorClock.Node
{
    class ApplicationLogic
    {
        public decimal balance;

        public void IncreaseBalance(decimal newBalance)
        {
            balance += newBalance;
        }

        public void DecreaseBalance(decimal newBalance)
        {
            balance -= newBalance;
        }
    }

    
}
