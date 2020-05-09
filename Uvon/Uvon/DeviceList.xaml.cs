using MonkeyCache.FileStore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
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
            InitializeComponent();

            address_bytes = Encoding.UTF8.GetBytes(Devices.GetLocalIPAddress());
            var devices_list = Addresses.addresses;

            Devices_list.ItemsSource = devices_list;
        }


        /// <summary>
        /// Works when user clicks on Add button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnAdd(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;
            string address = mi.CommandParameter.ToString();
            Debug.WriteLine("On more " + address);

            if (Addresses.favorites.Contains(address))
            {
                await DisplayAlert("Fine", "You have already added this address to your favorite list", "OK");
            }
            else
            {
                Addresses.favorites.Add(address);
                Barrel.Current.Empty("devices");
                Barrel.Current.Add(key: "devices", data: Addresses.favorites, expireIn: TimeSpan.FromDays(1));

                await DisplayAlert("Congratulations", "You added this address to your favorite list", "OK");
            }
        }


        /// <summary>
        /// Works when user tapps on item from List View
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            string address = e.Item.ToString();
            ip = IPAddress.Parse(address);

            Devices.SendCheckingSignal(this.port, this.ip, this.address_bytes);

            var mainPage = new MainPage(ip);
            await Navigation.PushAsync(mainPage);
        }

    }
}