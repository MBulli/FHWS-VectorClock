using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using VectorClock.Common;

namespace VectorClock.Commander
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            using (UdpClient client = new UdpClient())
            {
                Message msg = MessageFactory.Control.Shutdown();
                byte[] data = MessageSerializer.Serialze(msg);
                
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("10.31.52.46"), 1337);
                client.Send(data, data.Length, endPoint);
            }
        }
    }
}
