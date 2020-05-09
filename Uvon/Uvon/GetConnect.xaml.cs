using MonkeyCache.FileStore;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Uvon
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GetConnect : ContentPage
    {
        private IPAddress ip;
        private int port = 55554;
        Devices devs;

        private byte[] address_bytes = new byte[1024];
        private static string address = "192.168.1.1";
        private int interval;

        public GetConnect()
        {
            InitializeComponent();
            Barrel.ApplicationId = "Uvon";
            Addresses.favorites = Barrel.Current.Get<ObservableCollection<string>>("devices");  //getting my list from cashes

            if (Addresses.favorites == null)
            {
                Addresses.favorites = new ObservableCollection<string>();
            }
            warning.Text = "WARNING!!! UV LEDs\nHigh intensity ultraviolet light. Avoid exposure to eyes/skin. Do not look direclty at light, go out from room while UV is on.";
            instruction.Text = "Welcome to Uvon. If you want to connect your robot turn on it and enter the ip address to connect with him. If you want to do automatic ip addresses detecion please enter interval of search";
            address_bytes = Encoding.UTF8.GetBytes(Devices.GetLocalIPAddress());
            devs = new Devices();
        }

        private void ChangedText(object sender, EventArgs e)
        {
            address = user_input.Text;
        }

        /// <summary>
        /// Works when the page appears
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            user_input.Text = "";
            Addresses.addresses.Clear();
        }

        /// <summary>
        /// Works when the page disappers
        /// </summary>
        /// <returns></returns>
        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();
            return true;
        }


        /// <summary>
        /// Works when user clicks on Submit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void submit_Clicked(object sender, EventArgs e)
        {
            bool validateIP = IPAddress.TryParse(address, out ip);
            string[] add = address.ToString().Split('.');

            if (!validateIP || add.Length != 4)
            {
                await DisplayAlert("Warning!", "Please, enter valid ip address.", "OK");
            }
            else
            {
                Devices.SendCheckingSignal(this.port, this.ip, this.address_bytes);
                var mainPage = new MainPage(ip);
                await Navigation.PushAsync(mainPage);
            }
        }


        /// <summary>
        /// Works when user clicks on Scan button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void scan_Clicked(object sender, EventArgs e)
        {
            ActivityIndicator activityIndicator = new ActivityIndicator { IsRunning = true, IsVisible = true };

            var good = int.TryParse(user_input.Text, out interval);
            if (!good || interval > 255)
            {
                await DisplayAlert("Warning!", "Please, enter number in [0,255].", "OK");
            }
            else
            {
                Ping myping = new Ping();

                //var address = Devices.GetLocalIPAddress();
                string address = "192.168.1.1";
                if (interval == 0)
                {
                    interval = 255;
                }
                Scanning(address, myping, interval);
                var devicelist = new DeviceList(devs.Addresses);
                await Navigation.PushAsync(devicelist);
            }
        }


        /// <summary>
        /// Scans LAN connected devices and returnes list of IP addresses
        /// </summary>
        /// <param name="address"></param>
        /// <param name="myping"></param>
        /// <param name="interval"></param>
        private async void Scanning(string address, Ping myping, int interval)
        {
            await Task.Run(() =>
            {
                string[] address_array = address.Split('.');

                for (int i = 0; i <= interval; i++)
                {
                    try
                    {
                        PingReply reply;
                        reply = myping.Send(address_array[0] + '.' + address_array[1] + '.' + address_array[2] + '.' + i.ToString(), 100);

                        if (reply != null && reply.Status == IPStatus.Success)
                        {
                            Debug.WriteLine("Status :  " + reply.Status + ",  Time : " + reply.RoundtripTime.ToString() + ",  Address : " + reply.Address + "\n");
                            //devs.Addresses.Add(reply.Address.ToString());

                            Addresses.addresses.Add(reply.Address.ToString());
                        }
                    }
                    finally
                    {

                    }
                }

            });
        }
    }
}