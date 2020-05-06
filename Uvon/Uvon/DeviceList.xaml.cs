using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Uvon
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceList : ContentPage
    {
        private IPAddress ip;
        private int port = 55554;
        private byte[] address_bytes = new byte[1024];

        public DeviceList(List<string> devices)
        {
            address_bytes = Encoding.UTF8.GetBytes(Devices.GetLocalIPAddress());
            Label header = new Label
            {
                Text = "Available devices",
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                HorizontalOptions = LayoutOptions.Center
            };

            ListView listView = new ListView() { ItemsSource = Addresses.addresses};
            listView.ItemTapped += ListView_ItemTapped;
            this.Content = new StackLayout { Children = { header, listView} };
        }

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            string address = e.Item.ToString();
            ip = IPAddress.Parse(address);

            SendCheckingSignal();

            var mainPage = new MainPage(ip);
            await Navigation.PushAsync(mainPage);
        }

        private async void SendCheckingSignal()
        {
            await Task.Run(() =>
            {
                UdpClient client = new UdpClient(this.port);
                IPEndPoint ipendpoint = new IPEndPoint(this.ip, this.port);

                client.Send(address_bytes, address_bytes.Length, ipendpoint);
            });
        }
    }
}