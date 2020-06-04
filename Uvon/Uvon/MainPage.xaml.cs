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
        private string[] signal = new string[4];     //first is motor, second is uv

        IPAddress address;
        CancellationTokenSource cancelMotorPreview;
        CancellationToken token;

        public MainPage(IPAddress ip)
        {
            InitializeComponent();

            if (DesignMode.IsDesignModeEnabled)
            {
                return;
            }

            cancelMotorPreview = new CancellationTokenSource();        
            token = cancelMotorPreview.Token;

            address_bytes = Encoding.UTF8.GetBytes(Devices.GetLocalIPAddress());

            address = ip;
            var im = ImageSource.FromFile("drawable/welcome.png");

            signal[0] = "00";                                               //Motor's signal
            signal[1] = "00";                                               //ON/OFF signal
            signal[2] = "0";                                                //UV 1
            signal[3] = "0";                                                //UV 2
            
            preview.Source = im;
            //info_view.Text = Devices.GetLocalIPAddress();  //uncomment after text Label is uncommented in mainpage.xaml

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
            if(signal[1] == "ON")
            {
                signal[1] = "OFF";
            }
            signal[2] = "0";
            signal[3] = "0";

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


        #region Buttons pressed events handlers

        /// <summary>
        /// Left pressed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Left_Pressed(object sender, EventArgs e)
        {
            signal[0] = "3";
        }

        /// <summary>
        /// Go pressed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Go_Pressed(object sender, EventArgs e)
        {
            signal[0] = "1";
        }

        /// <summary>
        /// Back pressed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Pressed(object sender, EventArgs e)
        {
            signal[0] = "2";
        }

          
        /// <summary>
        /// Right pressed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Right_Pressed(object sender, EventArgs e)
        {
            signal[0] = "4";
        }

        #endregion

        #region Buttons Released event handlers

        /// <summary>
        /// Left released event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Left_Released(object sender, EventArgs e)
        {
            signal[0] = "0";
        }

        /// <summary>
        /// Go released event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Go_Released(object sender, EventArgs e)
        {
            signal[0] = "0";
        }

        /// <summary>
        /// Back released event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Released(object sender, EventArgs e)
        {
            signal[0] = "0";
        }

        /// <summary>
        /// Right released event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Right_Released(object sender, EventArgs e)
        {
            signal[0] = "0";
        }

        #endregion

        /// <summary>
        ///  Works when user clicks on Switch button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void turn_Clicked(object sender, EventArgs e)
        {
            if(turn.Text == "Turn On")
            {
                signal[1] = "ON";
                turn.Text = "Turn Off";
                motors_box.BackgroundColor = Color.Green;
            }
            else if(turn.Text == "Turn Off")
            {
                signal[1] = "OFF";
                turn.Text = "Turn On";
                motors_box.BackgroundColor = Color.Red;
            }
        }

        /// <summary>
        /// Works when user clicks on Disconnect button, and closes all connections with server/robot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void disconnect_Clicked(object sender, EventArgs e)
        {
            signal[0] = "34";
            signal[2] = "0";
            signal[3] = "0";

            if(signal[1] == "ON")
            {
                signal[1] = "OFF";
            }

            Thread.Sleep(100);

            if(cancelMotorPreview != null)
            {
                cancelMotorPreview.Cancel();
            }

            await Navigation.PushAsync(new GetConnect());
        }


        #endregion

        /// <summary>
        /// UV light 1st level turn on/off event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void switch_uv_1_Toggled(object sender, ToggledEventArgs e)
        {
            if (switch_uv_1.IsToggled)
            {
                var accept = await DisplayAlert("Warning!", "You are going to enable UV light's 1st level..", "Enable", "Discard");
                if (accept)
                {
                    signal[2] = "1";
                    uv_light1_box.BackgroundColor = Color.Green;
                }
                else
                {
                    switch_uv_1.IsToggled = false;
                }
            }
            else
            {
                signal[2] = "0";
                switch_uv_2.IsToggled = false;
                uv_light1_box.BackgroundColor = Color.Red;
            }
        }

        /// <summary>
        /// UV light 2nd level turn on/off event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void switch_uv_2_Toggled(object sender, ToggledEventArgs e)
        {
            if (switch_uv_2.IsToggled)
            {
                if (!switch_uv_1.IsToggled)
                {
                    await DisplayAlert("Warning!", "Please, enable UV level 1 at first...", "OK");
                    switch_uv_2.IsToggled = false;
                    return;
                }

                var accept = await DisplayAlert("Warning!", "You are going to enable UV light's 2nd level..", "Enable", "Discard");
                if (accept)
                {
                    signal[3] = "1";
                    uv_light2_box.BackgroundColor = Color.Green;
                }
                else
                {
                    switch_uv_2.IsToggled = false;
                }
            }
            else
            {
                uv_light2_box.BackgroundColor = Color.Red;
                signal[3] = "0";
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
                signal_data = Encoding.UTF8.GetBytes(signal[0] + "|" + signal[1] + "|" + signal[2] + "|" + signal[3]);
                client.Send(signal_data, signal_data.Length, ip);

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        client.Close();
                        return;
                    }
                    signal_data = Encoding.UTF8.GetBytes(signal[0] + "|" + signal[1] + "|" + signal[2] + "|" + signal[3]);
                    client.Send(signal_data, signal_data.Length, ip);
                    Debug.WriteLine("Was sent: " + signal[0] + " " + signal[1] + " " + signal[2] + " " + signal[3]);
                    Thread.Sleep(10);
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