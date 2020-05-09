using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientTest
{
    class Program
    {
        static Devices devs;
        static void Main(string[] args)
        {                                                       //Not included in project.
            Console.WriteLine("Starting server...");            //Here i tested some functions.

           Console.WriteLine(GetName(IPAddress.Parse("192.168.1.6")));
            devs = new Devices();
            //devs.Addresses.Add("erku");
            Ping_all();
            foreach (var i in devs.Addresses)
            {
                Console.WriteLine(i);
            }
        }


        public static string GetLocalIPAddress()
        {
            string port = null;
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                Console.WriteLine(ip);
                port = ip.ToString();
            }
            return port;
        }

        public static void Ping_all()
        {

            string machine_ip = GetLocalIPAddress();

            //Extracting and pinging all other ip's.
            string[] array = machine_ip.Split('.');

            for (int i = 2; i <= 255; i++)
            {

                string ping_ip_address = array[0] + "." + array[1] + "." + array[2] + "." + i;
                //time in milliseconds           
                Ping(ping_ip_address, 4, 5000);

            }

        }

        public static void Ping(string host, int attempts, int timeout)
        {
            for (int i = 0; i < attempts; i++)
            {
                new Thread(delegate ()
                {
                    try
                    {
                        Ping ping = new Ping();
                        ping.PingCompleted += new PingCompletedEventHandler(PingCompleted);
                        ping.SendAsync(host, timeout, host);
                    }
                    catch
                    {

                    }
                }).Start();
            }
        }

        private static void PingCompleted(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply != null && e.Reply.Status == IPStatus.Success)
            {
                string hostname = GetHostName(ip);
                string macaddres = GetMacAddress(ip);
                string[] arr = new string[3];

                //store all three parameters to be shown on ListView
                arr[0] = ip;
                arr[1] = hostname;
                arr[2] = macaddres;

                //Console.WriteLine("Ip address is: " + arr[0] + ", Hostname is: " + arr[1] + ",Macaddress is: " + arr[2] + "\n");
                if (devs.Addresses.Contains(arr[0]) == false)
                {
                    devs.Addresses.Add(arr[0]);
                    devs.Hostnames.Add(arr[1]);
                    devs.Macaddresses.Add(arr[2]);
                }
            }
            else
            {
                // MessageBox.Show(e.Reply.Status.ToString());
            }
        }

        public static string GetName(IPAddress ip)
        {
            string name = Dns.GetHostByAddress(ip).HostName;
            return name;
        }

        public static string GetHostName(string ipAddress)
        {
            try
            {
                IPHostEntry entry = Dns.GetHostEntry(ipAddress);
                if (entry != null)
                {
                    return entry.HostName;
                }
            }
            catch (SocketException)
            {
                // MessageBox.Show(e.Message.ToString());
            }

            return null;
        }

        public static string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process Process = new System.Diagnostics.Process();
            Process.StartInfo.FileName = "arp";
            Process.StartInfo.Arguments = "-a " + ipAddress;
            Process.StartInfo.UseShellExecute = false;
            Process.StartInfo.RedirectStandardOutput = true;
            Process.StartInfo.CreateNoWindow = true;
            Process.Start();
            string strOutput = Process.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6]
                         + "-" + substrings[7] + "-"
                         + substrings[8].Substring(0, 2);
                return macAddress;
            }

            else
            {
                return "OWN Machine";
            }
        }
    }

    class Devices
    {
        private List<string> addresses;
        private List<string> hostnames;
        private List<string> macaddresses;

        public List<string> Addresses
        {
            get
            {
                return addresses;
            }
            set
            {
                addresses.Add(value[0]);
            }
        }

        public List<string> Hostnames
        {
            get
            {
                return hostnames;
            }
            set
            {
                hostnames.Add(value[0]);
            }
        }

        public List<string> Macaddresses
        {
            get
            {
                return macaddresses;
            }
            set
            {
                macaddresses.Add(value[0]);
            }
        }
        public Devices()
        {
            addresses = new List<string>();
            hostnames = new List<string>();
            macaddresses = new List<string>();
        }
    }
}
