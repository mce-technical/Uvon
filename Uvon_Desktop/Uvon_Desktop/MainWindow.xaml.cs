﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Uvon_Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int scan_interval = 250;
        private IPAddress my_address, robot_address;
        private int port = 55554;
        private byte[] address_bytes = new byte[1024];
        private Ping myping;

        public MainWindow()
        {
            InitializeComponent();

            warning_text.Text = "WARNING!!! UV LEDs\nHigh intensity ultraviolet light. Avoid exposure to eyes/skin. Do not look direclty at light, go out from room while UV is on.";
            instruction_text.Text = "Welcome to Uvon. If you want to connect your robot turn on it and enter the ip address to connect with him. If you want to do automatic ip addresses detecion please enter interval of search. ";

            available_devices.ItemsSource = Addresses.addresses;

            user_input.GotFocus += Textbox_GotFocus;
            user_input.LostFocus += Textbox_LostFocus;

            my_address = Devices.GetLocalIPAddress();
            Debug.WriteLine(my_address);
            address_bytes = Encoding.UTF8.GetBytes(my_address.ToString());

            if (Addresses.addresses.Count == 0)
            {
                myping = new Ping();
                Scanning(my_address.ToString(), myping, 25);
            }
        }

        private void Textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(user_input.Text))
            {
                user_input.Text = "Please, enter ip address or scan interval";
            }
        }

        private void Textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (user_input.Text == "Please, enter ip address or scan interval")
            {
                user_input.Text = "";
            }
        }

        private void Autoconnect_Click(object sender, RoutedEventArgs e)
        {
            //Task.Run(() =>
            //{
            //    foreach (var x in Addresses.addresses)
            //    {
            //        UdpClient client = new UdpClient(55556);
            //        IPEndPoint ip = null;
            //        Devices.SendCheckingSignal(this.port, IPAddress.Parse(x), this.address_bytes);
            //        Debug.WriteLine("Current ip " + x);
            //        Thread.Sleep(500);
            //        var bytes = client.Receive(ref ip);
            //        if (bytes != null)
            //        {
            //            ControlPanel panel = new ControlPanel(my_address, IPAddress.Parse(x));
            //            panel.Show();

            //            break;
            //        }
            //    }
            //    this.Close();
            //});       
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
                Scanning(my_address.ToString(), myping, scan_interval);
            }
        }

        /// <summary>
        /// Connects to device with given Ip Address.
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
                MessageBox.Show("Please, enter valid ip address.", "Warning!!");
            }
            else
            {
                Devices.SendCheckingSignal(this.port, this.robot_address, this.address_bytes);

                ControlPanel panel = new ControlPanel(my_address, robot_address);
                panel.Show();
                this.Close();
            }
        }

        /// <summary>
        /// Connects to robot from the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void available_devices_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string robo_address;
            try
            {
                robo_address = ((FrameworkElement)e.OriginalSource).DataContext.ToString();          //gets the clicked element content
            }
            catch
            {
                robo_address = "127.0.0.1";
            }
            bool validateIP = IPAddress.TryParse(robo_address, out robot_address);

            Devices.SendCheckingSignal(this.port, this.robot_address, this.address_bytes);

            ControlPanel panel = new ControlPanel(my_address, robot_address);
            panel.Show();
            this.Close();
        }


        /// <summary>
        /// Scanning network to detect available devices, which are connected to WLAN
        /// </summary>
        /// <param name="address"></param>
        /// <param name="myping"></param>
        /// <param name="interval"></param>
        private async void Scanning(string address, Ping myping, int interval)
        {
            await Task.Run(() =>
            {
                string[] address_array = address.Split('.');

                for (int i = 1; i <= interval; i++)
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
                    }
                    finally
                    {

                    }
                }

            });
        }
    }
}