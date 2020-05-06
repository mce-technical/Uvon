using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace Uvon
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
        public static string GetLocalIPAddress()
        {
            string address = null;

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                Debug.WriteLine(ip);
                address = ip.ToString();
                break;
            }
            return address;
        }

    }

    public static class Addresses
    {
        public static  ObservableCollection<string> addresses = new ObservableCollection<string>();
    }
}
