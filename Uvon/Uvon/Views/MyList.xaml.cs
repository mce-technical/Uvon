using MonkeyCache.FileStore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Uvon.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyList : ContentPage
    {
        private IPAddress robot_ip;
        private int check_port = 55554;
        private int confirm_port = 45732;
        private string confirm_message = "iletyouconnectme";
        private byte[] own_address_bytes = new byte[1024];

        ObservableCollection<string> addr_values;

        public MyList()
        {
            InitializeComponent();

            loading.IsVisible = true;
            loading.IsRunning = false;

            addr_values = new ObservableCollection<string>();
            foreach(var x in Addresses.myEditableAddresses.Values)
            {
                addr_values.Add(x);
            }
            My_list.ItemsSource = addr_values;
            own_address_bytes = Encoding.UTF8.GetBytes(Devices.GetLocalIPAddress());
        }

        /// <summary>
        /// Works when user clicks on favorite list item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            loading.IsRunning = true;

            string address = Addresses.myEditableAddresses.FirstOrDefault(x => x.Value == e.Item.ToString()).Key;
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

        /// <summary>
        /// Works when user clicks on Add button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDelete(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;
            var address_value = mi.CommandParameter.ToString();
            string address_key = Addresses.myEditableAddresses.FirstOrDefault(x => x.Value == address_value).Key;
            
            if (Addresses.favoritesAddresses.Contains(address_key))
            {
                addr_values.Remove(address_value);

                Addresses.favoritesAddresses.Remove(address_key);
                Addresses.myEditableAddresses.Remove(address_key);
                
                Barrel.Current.Empty(address_key);
                Barrel.Current.Empty("devices");
                Barrel.Current.Add(key: "devices", data: Addresses.favoritesAddresses, expireIn: TimeSpan.FromDays(100));

                //await DisplayAlert("Congratulations", "You have deleted this address from your favorite list", "OK");
            }
        }


        private async void OnEdit(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Name", "Give a name to this device");
            var mi = (MenuItem)sender;
            var address_value = mi.CommandParameter.ToString();
            if (result != null)
            {
                Addresses.myEditableAddresses[address_value] = result;
                int index = addr_values.IndexOf(address_value);
                addr_values[index] = result;
                Barrel.Current.Add(key: address_value, data: result, expireIn: TimeSpan.FromDays(100));
            }
        }

        /// <summary>
        /// Clears favorite list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clear_Clicked(object sender, EventArgs e)
        {
            addr_values.Clear();
            Addresses.myEditableAddresses.Clear();
            Addresses.favoritesAddresses.Clear();
            Barrel.Current.EmptyAll();
        }
    }
}