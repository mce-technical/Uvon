using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Uvon_Desktop
{
    class Devices
    {
        public Devices()
        {
            Console.WriteLine("This class still contains only static methods");
        }
        /// <summary>
        /// Getting local ip address
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetLocalIPAddress()
        {
            IPAddress address = null;

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Debug.WriteLine("Own ip address is: " + ip);
                    address = ip;
                }
            }
            return address;
        }

        /// <summary>
        /// Sends checking signal to given host
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ip"></param>
        /// <param name="address_bytes"></param>
        public static async void SendCheckingSignal(int port, IPAddress ip, byte[] address_bytes)
        {
            await Task.Run(() =>
            {
                UdpClient client = new UdpClient(port);
                IPEndPoint ipendpoint = new IPEndPoint(ip, port);

                client.Send(address_bytes, address_bytes.Length, ipendpoint);
                client.Client.Close();
                client.Close();
                client = null;
            });
        }
    }

    public static class Addresses
    {
        public static ObservableCollection<string> addresses = new ObservableCollection<string>();
    }
}
