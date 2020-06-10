using MonkeyCache.FileStore;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Uvon
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceList : ContentPage
    {
        private IPAddress robot_ip;
        private int check_port = 55554;
        private int confirm_port = 45732;
        private string confirm_message = "iletyouconnectme";
        private byte[] own_address_bytes = new byte[1024];


        public DeviceList()
        {
            InitializeComponent();

            loading.IsVisible = true;
            loading.IsRunning = false;

            own_address_bytes = Encoding.UTF8.GetBytes(Devices.GetLocalIPAddress());
            var devices_list = Addresses.scanedAddresses;

            Devices_list.ItemsSource = devices_list;
            Task.Run(() =>
            {
                double i = 0;
                scan_progress.Progress = 0;

                while (GetConnect.index <= GetConnect.interval) 
                {
                    if (GetConnect.current_progress != i)
                    {
                        scan_progress.Progress = GetConnect.current_progress;
                        Debug.WriteLine(GetConnect.current_progress);
                        i = GetConnect.current_progress;
                    }
                }
                scan_progress.Progress = 0;
            });
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

            if (Addresses.favoritesAddresses.Contains(address))
            {
                await DisplayAlert("Fine", "You have already added this address to your favorite list", "OK");
            }
            else
            {
                Addresses.favoritesAddresses.Add(address);
                Addresses.myEditableAddresses.Add(address, address);
                Barrel.Current.Empty("devices");
                Barrel.Current.Add(key: "devices", data: Addresses.favoritesAddresses, expireIn: TimeSpan.FromDays(100));

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
            loading.IsRunning = true;

            string address = e.Item.ToString();
            robot_ip = IPAddress.Parse(address);

            Devices.SendCheckingSignal(this.check_port, this.robot_ip, this.own_address_bytes);

            Thread.Sleep(100);

            byte[] message = new byte[64];
            await Task.Run(() =>
            {
                message = Devices.StartConfirmation(confirm_port, 2000);

                if (Encoding.UTF8.GetString(message) == confirm_message)
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var mainPage = new MainPage(robot_ip);
                        await Navigation.PushAsync(mainPage);
                        return;
                    });
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await DisplayAlert("Warning!", "NO CONNECTION", "OK");
                        loading.IsRunning = false;
                        return;
                    });
                }
            });
        }

    }
}