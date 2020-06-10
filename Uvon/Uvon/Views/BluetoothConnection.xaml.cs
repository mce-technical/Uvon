using Plugin.BLE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Uvon.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothConnection : ContentPage
    {
        dynamic ble;
        dynamic adapter;
        public BluetoothConnection()
        {
            InitializeComponent();
            try
            {
                ble = CrossBluetoothLE.Current;
                adapter = CrossBluetoothLE.Current.Adapter;
            }
            catch(Exception e)
            {
                Debug.WriteLine("Exception is: " + e.Message);
            }

            //var state1 = ble.State;
            //state.Text = state1.ToString();
        }
    }
}