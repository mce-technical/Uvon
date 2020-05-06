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
using Xamarin.Forms.Xaml;
using System.Drawing;

namespace Uvon
{
    [DesignTimeVisible(true)]
    public partial class MainPage : ContentPage
    {
        byte[] signal_data;
        byte[] uv_data;
        private int signal_port = 55555;
        private int uv_port = 59472;
        private byte[] address_bytes;

        IPAddress address;
        CancellationTokenSource cancelMotorPreview, cancelUv;
        CancellationToken token, uv_token;

        public MainPage(IPAddress ip)
        {
            InitializeComponent();

            cancelMotorPreview = new CancellationTokenSource();        
            token = cancelMotorPreview.Token;

            address_bytes = Encoding.UTF8.GetBytes(Devices.GetLocalIPAddress());

            address = ip;
            var im = ImageSource.FromFile("drawable/welcome.png");
            signal_data = Encoding.UTF8.GetBytes("00");
           
            preview.Source = im;
            port_view.Text = Devices.GetLocalIPAddress();

            //Get_image();

            //SendSignal();

        }


        #region Page elements and buttons 
        /// <summary>
        /// Works when the page appears
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Send(this, "PreventLandscape");
 
        }

        /// <summary>
        /// Works when the page disappears
        /// </summary>
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            cancelMotorPreview.Cancel();

            uv_data = Encoding.UTF8.GetBytes("20");
            Thread.Sleep(100);
            cancelUv.Cancel();
            address = null;
        }


        /// <summary>
        /// Works when user clicks on Left button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Left_Clicked(object sender, EventArgs e)
        {
            port_view.Text = "Left";
            signal_data = Encoding.UTF8.GetBytes("03");
        }

        /// <summary>
        ///  Works when user clicks on Go button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Go_Clicked(object sender, EventArgs e)
        {
            port_view.Text = "Go";
            signal_data = Encoding.UTF8.GetBytes("01");
        }

        /// <summary>
        ///  Works when user clicks on Stop button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stop_Clicked(object sender, EventArgs e)
        {
            port_view.Text = "stop";
            signal_data = Encoding.UTF8.GetBytes("00");
        }

        /// <summary>
        ///  Works when user clicks on Back button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void back_Clicked(object sender, EventArgs e)
        {
            port_view.Text = "back";
            signal_data = Encoding.UTF8.GetBytes("02");
        }

        /// <summary>
        ///  Works when user clicks on Right button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Right_Clicked(object sender, EventArgs e)
        {
            port_view.Text = "right";
            signal_data = Encoding.UTF8.GetBytes("04");
        }

        /// <summary>
        ///  Works when user clicks on Switch button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void switch_Clicked(object sender, EventArgs e)
        {
            port_view.Text = "switch";
            //TO Do...
        }

        /// <summary>
        /// Disconnect from server/robot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void disconnect_Clicked(object sender, EventArgs e)
        {
            port_view.Text = "disconnect";
            cancelMotorPreview.Cancel();

            uv_data = Encoding.UTF8.GetBytes("20");
            Thread.Sleep(100);
            cancelUv.Cancel();

            await Navigation.PushAsync(new GetConnect());   
        }

        #endregion


        private async void switch_uv_Toggled(object sender, ToggledEventArgs e)
        {
            if (switch_uv.IsToggled)
            {
                var accept = await DisplayAlert("Warning!", "You are going to enable UV light..", "Enable" ,"Discard");
                if (accept)
                {
                    uv_data = Encoding.UTF8.GetBytes("00");
                    cancelUv = new CancellationTokenSource();
                    uv_token = cancelUv.Token;

                    Enable_Uv();
                }
                else
                {
                    switch_uv.IsToggled = false;
                }
            }
            else
            {
                uv_data = Encoding.UTF8.GetBytes("20");
                Thread.Sleep(100);
                cancelUv.Cancel();
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Sends signals to robot/server
        /// </summary>
        private async void SendSignal()
        {
            await Task.Run(() =>
            {
                UdpClient client = new UdpClient(signal_port);
                IPEndPoint ip = new IPEndPoint(address, signal_port);

                client.Send(signal_data, signal_data.Length, ip);

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        client.Close();
                        return;
                    }
                    client.Send(signal_data, signal_data.Length, ip);
                    Debug.WriteLine("Was sent: " + signal_data[1]);
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
                    Debug.WriteLine(bytes);

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

        
        /// <summary>
        /// Sends commands to keep UV light enabled
        /// </summary>
        private async void Enable_Uv()
        {
            UdpClient client_start = new UdpClient();
            IPEndPoint ipendpoint = new IPEndPoint(address,signal_port - 1);

            client_start.Send(address_bytes, address_bytes.Length, ipendpoint);
            client_start.Close();

            await Task.Run(() =>
            {
                UdpClient client = new UdpClient(uv_port);
                IPEndPoint ip = new IPEndPoint(address, uv_port);

                while (true)
                {
                    if (uv_token.IsCancellationRequested)
                    {
                        client.Close();
                        Debug.WriteLine("Canceled");
                        return;
                    }
                    client.Send(uv_data, uv_data.Length, ip);
                    Debug.WriteLine("UV enable sent");
                    Thread.Sleep(100);
                }
            });
        }
    }
}