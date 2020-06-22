using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Uvon
{
    /// <summary>
    /// Has a few static members to work with DNS and Ip Addresses.
    /// </summary>
    class Devices
    {
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

        /// <summary>
        /// Sends checking signal to given host
        /// </summary>
        /// <param name="port"></param>
        /// <param name="ip"></param>
        /// <param name="address_bytes"></param>
        public static async void SendCheckingSignal(ushort port, IPAddress ip, byte[] address_bytes)
        {
            await Task.Run(() =>
            {
                try
                {
                    UdpClient client = new UdpClient(port);
                    IPEndPoint ipendpoint = new IPEndPoint(ip, port);

                    client.Send(address_bytes, address_bytes.Length, ipendpoint);
                }
                catch(Exception e)
                {
                    Debug.WriteLine("Error message: " + e.ToString());
                }
            });
        }

        /// <summary>
        /// Starts to listen incoming confirmation messages
        /// </summary>
        /// <param name="confirm_port"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static byte[] StartConfirmation(ushort confirm_port, int timeout)
        {
            byte[] message = new byte[1024];
            try
            {
                UdpClient client = new UdpClient(confirm_port);
                IPEndPoint ipEnd = null;
                client.Client.ReceiveTimeout = timeout;
                message = client.Receive(ref ipEnd);
                Debug.WriteLine("BUffer length is: " + message.Length + "Message is: " + Encoding.UTF8.GetString(message));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error message: " + ex.Message);
            }
            return message;
        }
    }


    /// <summary>
    /// Static class to keep some data while application is working
    /// </summary>
    public static class Addresses
    {
        //Addresses from scan
        public static  ObservableCollection<string> scanedAddresses = new ObservableCollection<string>();

        //Favorite addresses
        public static ObservableCollection<string> favoritesAddresses = new ObservableCollection<string>();

        //These objects are [key, value] pairs. [address, name] format
        public static Dictionary<string, string> myEditableAddresses = new Dictionary<string, string>();
    }
}
