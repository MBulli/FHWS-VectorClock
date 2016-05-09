using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VectorClock.Commander.Helper;

namespace VectorClock.Commander.ViewModel
{
    public class NodeViewModel : PropertyChangedBase
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; OnNotifyPropertyChanged(); }
        }

        public NodeViewModel(string name = "")
        {
            this.name = name;
        }
    }
}
