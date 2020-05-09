using MonkeyCache.FileStore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Uvon.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyList : ContentPage
    {
        private IPAddress ip;
        private int port = 55554;
        private byte[] address_bytes = new byte[1024];

        public MyList()
        {
            InitializeComponent();
            My_list.ItemsSource = Addresses.favorites;     
        }

        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            string address = e.Item.ToString();
            ip = IPAddress.Parse(address);

            Devices.SendCheckingSignal(this.port, this.ip, this.address_bytes);

            var mainPage = new MainPage(ip);
            await Navigation.PushAsync(mainPage);
        }

        /// <summary>
        /// Works when user clicks on Add button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnDelete(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;
            string address = mi.CommandParameter.ToString();
            Debug.WriteLine("On more " + address);

            if (Addresses.favorites.Contains(address))
            {
                Addresses.favorites.Remove(address);
                Barrel.Current.Empty("devices");
                Barrel.Current.Add(key: "devices", data: Addresses.favorites, expireIn: TimeSpan.FromDays(1));

                await DisplayAlert("Congratulations", "You deleted this address from your favorite list", "OK");
            }
        }
    }
}