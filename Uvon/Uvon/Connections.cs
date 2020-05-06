using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using Xamarin.Forms;

namespace Uvon
{
    class Connections
    {
		static bool terminateThread = false;

		public static void Send_signal(int port, byte[] data)
		{
			UdpClient client = new UdpClient(port);
			IPEndPoint ip = new IPEndPoint(IPAddress.Loopback, port);

			while (true)
			{
				if (terminateThread == true)
				{
					break;
				}
				client.Send(data, data.Length, ip);
				Thread.Sleep(2);
			}
			Thread.Sleep(900);
			terminateThread = false;
			client.Close();
		}
    }
}
