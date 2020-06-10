using MonkeyCache.FileStore;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Uvon
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GetConnect : ContentPage
    {
        private IPAddress robot_ip;
        private int check_port = 55554;
        private int confirm_port = 45732;
        private string confirm_message = "iletyouconnectme";

        private byte[] own_address_bytes = new byte[1024];
        private static string address_input = "192.168.1.1";
        public static double interval;
        public static double current_progress = 0;
        public static double index = 0;

        public GetConnect()
        {
            InitializeComponent();
           

            loading.IsVisible = true;
            loading.IsRunning = false;

            NavigationPage.SetHasBackButton(this, false);

            Barrel.ApplicationId = "Uvon";

            Addresses.favoritesAddresses = Barrel.Current.Get<ObservableCollection<string>>("devices");  //getting my list from cashes


            if (Addresses.favoritesAddresses == null)
            {
                Addresses.favoritesAddresses = new ObservableCollection<string>();
            }

            warning.Text = "WARNING!!! UV LEDs\nHigh intensity ultraviolet light. Avoid exposure to eyes/skin. Do not look direclty at light, go out from room while UV is on.";
            instruction.Text = "Welcome to Uvon. If you want to connect your robot turn on it and enter the ip address to connect with him. If you want to do automatic ip addresses detecion please enter interval of search";
            own_address_bytes = Encoding.UTF8.GetBytes(Devices.GetLocalIPAddress());

            foreach (var it in Addresses.favoritesAddresses)
            {
                var x = Barrel.Current.Get<string>(it);
                if (!Addresses.myEditableAddresses.Keys.Contains(it))
                {
                    if (x != null)
                    {
                        Addresses.myEditableAddresses.Add(it, x);
                    }
                    else
                    {
                        Addresses.myEditableAddresses.Add(it, it);
                    }
                }
            }
        }

        private void ChangedText(object sender, EventArgs e)
        {
            address_input = user_input.Text;
        }

        /// <summary>
        /// Works when the page appears
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            user_input.Text = "";
            Addresses.scanedAddresses.Clear();
        }

        /// <summary>
        /// Works when the page appears
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            loading.IsRunning = false;
            user_input.Text = "";
            Addresses.scanedAddresses.Clear();

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
        /// To add device in favourit'se list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Add_Clicked(object sender, EventArgs e)
        {
            bool validateIP = IPAddress.TryParse(address_input, out robot_ip);
            string[] add = address_input.ToString().Split('.');
            if (!validateIP || add.Length != 4)
            {
                await DisplayAlert("Warning!", "Please, enter valid ip address.", "OK");
            }
            else
            {
                if (Addresses.favoritesAddresses.Contains(address_input))
                {
                    await DisplayAlert("Fine", "You have already added this address to your favorite list", "OK");
                }
                else
                {
                    Addresses.favoritesAddresses.Add(address_input);
                    Addresses.myEditableAddresses.Add(address_input, address_input);
                    Barrel.Current.Empty("devices");
                    Barrel.Current.Add(key: "devices", data: Addresses.favoritesAddresses, expireIn: TimeSpan.FromDays(100));

                    await DisplayAlert("Congratulations", "You added this address to your favorite list", "OK");
                }
            }
        }

        /// <summary>
        /// Works when user clicks on Submit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Submit_Clicked(object sender, EventArgs e)
        {
            bool validateIP = IPAddress.TryParse(address_input, out robot_ip);
            string[] add = address_input.ToString().Split('.');

            loading.IsRunning = true;

            if (!validateIP || add.Length != 4)
            {
                await DisplayAlert("Warning!", "Please, enter valid ip address.", "OK");
                loading.IsRunning = false;
            }
            else
            {
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


        /// <summary>
        /// Works when user clicks on Scan button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Scan_Clicked(object sender, EventArgs e)
        {
            var good = double.TryParse(user_input.Text, out interval);
            if (!good || interval > 255)
            {
                await DisplayAlert("Warning!", "Please, enter number in [0,255].", "OK");
            }
            else
            {
                Ping myping = new Ping();

                //var address = Devices.GetLocalIPAddress();
                var address = "192.168.11.6";

                if (interval == 0)
                {
                    interval = 255;
                }
                Scanning(address, myping, interval);
                var devicelist = new DeviceList();
                await Navigation.PushAsync(devicelist);
            }
        }


        /// <summary>
        /// Scans LAN connected devices and returnes list of IP addresses
        /// </summary>
        /// <param name="address"></param>
        /// <param name="myping"></param>
        /// <param name="interval"></param>
        private async void Scanning(string address, Ping myping, double interval)
        {
            await Task.Run(() =>
            {
                string[] address_array = address.Split('.');
                double min_growth = 1.0 / GetConnect.interval;
                for (int i = 0; i <= interval; i++)
                {
                    try
                    {
                        PingReply reply;
                        reply = myping.Send(address_array[0] + '.' + address_array[1] + '.' + address_array[2] + '.' + i.ToString(), 100);

                        if (reply != null && reply.Status == IPStatus.Success)
                        {
                            Debug.WriteLine("Status :  " + reply.Status + ",  Time : " + reply.RoundtripTime.ToString() + ",  Address : " + reply.Address + "\n");

                            Addresses.scanedAddresses.Add(reply.Address.ToString());
                        }
                    }
                    finally
                    {
                        current_progress += min_growth;
                        index++;
                    }
                }

            });
        }
    }
}