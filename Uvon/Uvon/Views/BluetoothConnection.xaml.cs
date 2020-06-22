using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Uvon.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothConnection : ContentPage
    {
        IBluetoothLE ble;
        IAdapter adapter;
        ObservableCollection<IDevice> devices_list;

        public BluetoothConnection()
        {
            InitializeComponent();

            devices_list = new ObservableCollection<IDevice>();
            Devices_list.ItemsSource = devices_list;

            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            var state1 = ble.State;
            state.Text = state1.ToString();      
        }


        private async void Scan_Clicked(object sender, EventArgs e)
        {
            try
            {
                adapter.DeviceDiscovered += (s, a) =>
                {
                    devices_list.Add(a.Device);
                    Debug.WriteLine(a.Device.AdvertisementRecords.ToString());
                };
                if (!ble.Adapter.IsScanning)
                {
                    await adapter.StartScanningForDevicesAsync();
                }
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.Message);
            }
        }

        private void Devices_list_ItemTapped(object sender, ItemTappedEventArgs e)
        {

        }

        private void ConnectDevice(object sender, EventArgs e)
        {

        }
    }
}