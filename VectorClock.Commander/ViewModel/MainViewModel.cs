using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VectorClock.Commander.Helper;

namespace VectorClock.Commander.ViewModel
{
    public class MainViewModel : PropertyChangedBase
    {
        private string title = "VectorClock";
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
            Node1 = new NodeViewModel("mjverteil01", System.Net.IPAddress.Parse("10.10.29.21"));
            Node2 = new NodeViewModel("mjverteil02", System.Net.IPAddress.Parse("10.10.29.142"));
            Node3 = new NodeViewModel("mjverteil03", System.Net.IPAddress.Parse("10.10.29.67"));

            CheckNodeConnectivities(); //TODO: Timer


        }

        public void CheckNodeConnectivities()
        {
            Task.Run(async () => await Node1.CheckConnectivity());
            Task.Run(async () => await Node2.CheckConnectivity());
            Task.Run(async () => await Node3.CheckConnectivity());
        }

    }
}
