﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Uvon_Desktop
{
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
                if(ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Debug.WriteLine(ip);
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
                Debug.WriteLine("Was sent");
            });
        }
    }

    public static class Addresses
    {
        public static ObservableCollection<string> addresses = new ObservableCollection<string>();
    }
}