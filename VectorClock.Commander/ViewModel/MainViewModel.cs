using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectorClock.Commander.Helper;

namespace VectorClock.Commander.ViewModel
{
    public class MainViewModel : PropertyChangedBase
    {
        private string title = "Hallo";
        public string Title
        {
            get { return title; } 
            set { title = value; OnNotifyPropertyChanged(); }
        }


        public NodeViewModel Node1 { get; set; }
        public NodeViewModel Node2 { get; set; }
        public NodeViewModel Node3 { get; set; }

        public MainViewModel()
        {
            Node1 = new NodeViewModel("mjverteil01");
            Node2 = new NodeViewModel("mjverteil02");
            Node3 = new NodeViewModel("mjverteil03");
        }

    }
}
