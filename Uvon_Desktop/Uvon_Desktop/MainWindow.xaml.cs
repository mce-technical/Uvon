using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Uvon_Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int scan_interval = 250;                //default interval to scan
        private IPAddress my_address, robot_address;    //my and robot's ip addresses
        private int port = 55554;                       //to send checking signal
        private byte[] address_bytes = new byte[1024];  //My ip address in bytes
        private Ping myping;                
        protected ControlPanel panel;

        public MainWindow()
        {
            InitializeComponent();

            warning_text.Text = "WARNING!!! UV LEDs\nHigh intensity ultraviolet light. Avoid exposure to eyes/skin. Do not look direclty at light, go out from room while UV is on.";
            instruction_text.Text = "Welcome to Uvon. If you want to connect your robot turn on it and enter the ip address to connect with him. If you want to do automatic ip addresses detecion please enter interval of search. ";

            available_devices.ItemsSource = Addresses.addresses;            //Those are devices connected to WLAN

            user_input.GotFocus += Textbox_GotFocus;                        //Works when user clicks on text input field
            user_input.LostFocus += Textbox_LostFocus;                      //Works when user moves mouse out of the text input field

            my_address = Devices.GetLocalIPAddress();                       //Gets machines own Ip Address
            Debug.WriteLine("My address is:" + my_address);
            address_bytes = Encoding.UTF8.GetBytes(my_address.ToString());  //Encodes ip address to bytes

            if (Addresses.addresses.Count == 0)
            {
                myping = new Ping();
                Scanning(my_address.ToString(), myping, 10);                //Scans WLAN 
            }
        }

        /// <summary>
        /// Mouse left click on text input field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(user_input.Text))
            {
                user_input.Text = "Please, enter ip address or scan interval";
            }
        }

        /// <summary>
        /// Mouse left click on everywhere out of the text input field
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (user_input.Text == "Please, enter ip address or scan interval")
            {
                user_input.Text = "";
            }
        }

        /// <summary>
        /// Automaticly connects to available server/robot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Autoconnect_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                foreach (var x in Addresses.addresses)
                {
                    UdpClient client = new UdpClient(55556);
                    IPEndPoint ip = null;
                    Devices.SendCheckingSignal(this.port, IPAddress.Parse(x), this.address_bytes);
                    Debug.WriteLine("Current ip: " + x);
                    Thread.Sleep(500);
                    try
                    {
                        var bytes = client.Receive(ref ip);
                        if (bytes != null)
                        {
                            ControlPanel panel = new ControlPanel(IPAddress.Parse(x));
                            panel.Show();

                            break;
                        }
                    }
                    catch
                    {
                        Debug.WriteLine("Something goes wrong..");
                    }
                }
                this.Close();
            });
        }

        /// <summary>
        /// Works when user clicks on Scan button. It scans which devices are connected to WLAN.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Scan_Click(object sender, RoutedEventArgs e)
        {
            var good = int.TryParse(user_input.Text, out scan_interval);

            if (!good || scan_interval > 255)
            {
                MessageBox.Show("Incorrect interval", "Warning!!");
            }
            else
            {
                myping = new Ping();

                if (scan_interval == 0)
                {
                    scan_interval = 255;
                }

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.Title = "Scanning...";
                    if (progress.Value != 0)
                    {
                        progress.Value = 0;
                    }
                }));
                Scanning(my_address.ToString(), myping, scan_interval);
            }
        }

        /// <summary>
        /// Connects to device with given Ip Address or with selected Ip address from List.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            bool validateIP = IPAddress.TryParse(user_input.Text, out robot_address);
            Debug.WriteLine("Robot address is: " + robot_address);
            string[] add = user_input.Text.Split('.');

            if (!validateIP || add.Length != 4)
            {
                if (available_devices.SelectedItem == null)
                {
                    MessageBox.Show("Please, enter valid ip address.", "Warning!!");
                }
                else
                {
                    Thread.Sleep(1000);
                    Connect(IPAddress.Parse(available_devices.SelectedItem.ToString()));
                }
            }
            else
            {
                Connect(this.robot_address);
            }
        }

        /// <summary>
        /// Sends checking signal and connectes with server/robot
        /// </summary>
        /// <param name="robotAddress"></param>
        private void Connect(IPAddress robotAddress)
        {
            Devices.SendCheckingSignal(this.port, robotAddress, this.address_bytes);

            panel = new ControlPanel(robotAddress);
            panel.Show();
            this.Close();
        }


        /// <summary>
        /// Scanning network to detect available devices, which are connected to WLAN
        /// </summary>
        /// <param name="address"></param>
        /// <param name="myping"></param>
        /// <param name="interval"></param>
        private async void Scanning(string address, Ping myping, double interval)
        {
            await Task.Run(() =>
            {
                string[] address_array = address.Split('.');

                for (int i = 2; i <= interval; i++)
                {
                    try
                    {
                        PingReply reply;    
                        reply = myping.Send(address_array[0] + '.' + address_array[1] + '.' + address_array[2] + '.' + i.ToString(), 100);

                        if (reply != null && reply.Status == IPStatus.Success)
                        {
                            Debug.WriteLine("Status :  " + reply.Status + ",  Time : " + reply.RoundtripTime.ToString() + ",  Address : " + reply.Address + "\n");

                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (!Addresses.addresses.Contains(reply.Address.ToString()))
                                {
                                    Addresses.addresses.Add(reply.Address.ToString());
                                }
                            }));
                        }
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            progress.Value += 100.0 / interval;
                        }));
                    }
                    finally
                    {
                        Debug.WriteLine("End connection... " + i);
                    }
                }
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.Title = "UVON";
                    progress.Value = 0;
                }));
            });
        }
    }
}
