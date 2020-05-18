using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Uvon
{
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        private int signal_port = 55555;
        private byte[] address_bytes;
        private string[] signal = new string[2];     //first is motor, second is uv

        IPAddress address;
        CancellationTokenSource cancelMotorPreview;
        CancellationToken token;

        public MainPage(IPAddress ip)
        {
            InitializeComponent();

            cancelMotorPreview = new CancellationTokenSource();        
            token = cancelMotorPreview.Token;

            address_bytes = Encoding.UTF8.GetBytes(Devices.GetLocalIPAddress());

            address = ip;
            var im = ImageSource.FromFile("drawable/welcome.png");
            signal[0] = "00";
            signal[1] = "00";
           
            preview.Source = im;
            //port_view.Text = Devices.GetLocalIPAddress();  //uncomment after text Label is uncommented in mainpage.xaml

            Get_image();    //Starting new task to get preview from server/robot

            SendSignal();   //Starting new task to send signal to control motors of robor.
        }


        #region Page elements and buttons 
        /// <summary>
        /// Works when the page appears
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            preview.Source = ImageSource.FromFile("drawable/welcome.png");
            MessagingCenter.Send(this, "PreventLandscape");
        }

        /// <summary>
        /// Works when the page disappears
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            signal[0] = "34";
            signal[1] = "00";

            Thread.Sleep(100);

            if (cancelMotorPreview != null)
            {
                cancelMotorPreview.Cancel();
            }
        }

        
        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();
            return true;
        }

        /// <summary>
        /// Works when user clicks on Left button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Left_Clicked(object sender, EventArgs e)
        {
            signal[0] = "03";
        }

        /// <summary>
        ///  Works when user clicks on Go button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Go_Clicked(object sender, EventArgs e)
        {
            signal[0] = "01";
        }

        /// <summary>
        ///  Works when user clicks on Stop button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stop_Clicked(object sender, EventArgs e)
        {
            signal[0] = "00";
        }

        /// <summary>
        ///  Works when user clicks on Back button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void back_Clicked(object sender, EventArgs e)
        {
            signal[0] = "02";
        }

        /// <summary>
        ///  Works when user clicks on Right button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Right_Clicked(object sender, EventArgs e)
        {
            signal[0] = "04";
        }

        /// <summary>
        ///  Works when user clicks on Switch button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void switch_Clicked(object sender, EventArgs e)
        {
            Debug.WriteLine("Switch");

            //TO Do...
        }

        /// <summary>
        /// Works when user clicks on Disconnect button, and closes all connections with server/robot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void disconnect_Clicked(object sender, EventArgs e)
        {
            signal[0] = "34";
            signal[1] = "00";

            Thread.Sleep(100);

            if(cancelMotorPreview != null)
            {
                cancelMotorPreview.Cancel();
            }

            await Navigation.PushAsync(new GetConnect());
        }


        #endregion

        /// <summary>
        /// UV light turn on/off event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void switch_uv_Toggled(object sender, ToggledEventArgs e)
        {
            if (switch_uv.IsToggled)
            {
                var accept = await DisplayAlert("Warning!", "You are going to enable UV light..", "Enable" ,"Discard");
                if (accept)
                {
                    signal[1] = "01";
                }
                else
                {
                    switch_uv.IsToggled = false;
                }
            }
            else
            {
                signal[1] = "00";
            }
        }

        /// <summary>
        /// Sends signals to robot/server
        /// </summary>
        private async void SendSignal()
        {
            byte[] signal_data;
            await Task.Run(() =>
            {
                UdpClient client = new UdpClient(signal_port);
                IPEndPoint ip = new IPEndPoint(address, signal_port);
                signal_data = Encoding.UTF8.GetBytes(signal[0] + "|" + signal[1]);
                client.Send(signal_data, signal_data.Length, ip);

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        client.Close();
                        return;
                    }
                    signal_data = Encoding.UTF8.GetBytes(signal[0] + "|" + signal[1]);
                    client.Send(signal_data, signal_data.Length, ip);
                    Debug.WriteLine("Was sent: " + signal[0] + " " + signal[1]);
                    Thread.Sleep(100);
                }
            });
        }

        /// <summary>
        /// Gets view from robot/server
        /// </summary>
        private async void Get_image()
        {
            byte[] bytes = new byte[1024];
            await Task.Run(() =>
            {
                UdpClient client = new UdpClient(signal_port + 1);
                IPEndPoint ip = null;

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        client.Close();
                        return;
                    }

                    bytes = client.Receive(ref ip);

                    //Parse to image
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        preview.Source = ConvertToImage(bytes);
                    });
                }
            });
        }


        /// <summary>
        /// Converts byte array to image source
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        private static ImageSource ConvertToImage(byte[] array)
        {
            return ImageSource.FromStream(() => new MemoryStream(array));
        }
    }
}