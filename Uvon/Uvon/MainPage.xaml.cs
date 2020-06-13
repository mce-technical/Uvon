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
        private int get_signal_port = 55555;
        private int get_status_port = 53784;
        private string[] signals_array = new string[5];     //first is motor, second is uv
        private string line_track_status = "0", battery_1_status = "0", battery_2_status = "0";
        double page_width;
        double element_size;
        IPAddress robot_address;
        CancellationTokenSource cancelConnections;
        CancellationToken token;

        public MainPage(IPAddress ip)
        {
            page_width = Application.Current.MainPage.Width;
            element_size = (page_width - 90) / 8;

            InitializeComponent();

            if (DesignMode.IsDesignModeEnabled)
            {
                return;
            }

            cancelConnections = new CancellationTokenSource();        
            token = cancelConnections.Token;

            robot_address = ip;
            var im = ImageSource.FromFile("drawable/welcome.png");

            signals_array[0] = "00";                                               //Motor's signal
            signals_array[1] = "00";                                               //Motor drivers ON/OFF signal
            signals_array[2] = "0";                                                //UV 1
            signals_array[3] = "0";                                                //UV 2
            signals_array[4] = "0";                                                //Line tracking mode on/off   
            preview.Source = im;

            //info_view.Text = Devices.GetLocalIPAddress();  //uncomment after text Label is uncommented in mainpage.xaml

            GetImage();    //Starting new task to get preview from server/robot

            SendSignal();   //Starting new task to send signal to control motors of robor.

            GetStatusInfo();
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

            signals_array[0] = "34";
            if(signals_array[1] == "ON")
            {
                signals_array[1] = "OFF";
            }
            signals_array[2] = "0";
            signals_array[3] = "0";
            signals_array[4] = "0";

            Thread.Sleep(100);

            if (cancelConnections != null)
            {
                cancelConnections.Cancel();
            }
        }


        //protected override bool OnBackButtonPressed()
        //{
        //    base.OnBackButtonPressed();
        //    return true;
        //}


        #region Buttons pressed events handlers

        /// <summary>
        /// Left pressed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Left_Pressed(object sender, EventArgs e)
        {
            signals_array[0] = "3";
        }

        /// <summary>
        /// Go pressed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Go_Pressed(object sender, EventArgs e)
        {
            signals_array[0] = "1";
        }

        /// <summary>
        /// Back pressed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Pressed(object sender, EventArgs e)
        {
            signals_array[0] = "2";
        }

          
        /// <summary>
        /// Right pressed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Right_Pressed(object sender, EventArgs e)
        {
            signals_array[0] = "4";
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
            signals_array[0] = "0";
        }

        /// <summary>
        /// Go released event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Go_Released(object sender, EventArgs e)
        {
            signals_array[0] = "0";
        }

        /// <summary>
        /// Back released event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Released(object sender, EventArgs e)
        {
            signals_array[0] = "0";
        }

        /// <summary>
        /// Right released event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Right_Released(object sender, EventArgs e)
        {
            signals_array[0] = "0";
        }

        #endregion

        /// <summary>
        ///  Works when user clicks on Switch button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Turn_Clicked(object sender, EventArgs e)
        {
            if(turn.Text == "Turn On")
            {
                signals_array[1] = "ON";
                turn.Text = "Turn Off";
                motors_box.BackgroundColor = Color.Green;
            }
            else if(turn.Text == "Turn Off")
            {
                signals_array[1] = "OFF";
                turn.Text = "Turn On";
                motors_box.BackgroundColor = Color.Red;
            }
        }


        /// <summary>
        /// Starts line tracking
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LineTracking_Clicked(object sender, EventArgs e)
        {
            if (lineTracking.Text == "Line tracking")
            {
                signals_array[4] = "1";
                lineTracking.Text = "Hand control";
                autopilot_box.BackgroundColor = Color.Green;
            }
            else if (lineTracking.Text == "Hand control")
            {
                signals_array[4] = "0";
                lineTracking.Text = "Line tracking";
                autopilot_box.BackgroundColor = Color.Red;
            }
        }

        /// <summary>
        /// Works when user clicks on Disconnect button, and closes all connections with server/robot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Disconnect_Clicked(object sender, EventArgs e)
        {
            signals_array[0] = "34";
            signals_array[2] = "0";
            signals_array[3] = "0";
            signals_array[4] = "0";

            if(signals_array[1] == "ON")
            {
                signals_array[1] = "OFF";
            }

            Thread.Sleep(100);

            if(cancelConnections != null)
            {
                cancelConnections.Cancel();
            }

            await Navigation.PushAsync(new GetConnect());
        }


        #endregion

        /// <summary>
        /// UV light 1st level turn on/off event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Switch_uv_1_Toggled(object sender, ToggledEventArgs e)
        {
            if (switch_uv_1.IsToggled)
            {
                var accept = await DisplayAlert("Warning!", "You are going to enable UV light's 1st level..", "Enable", "Discard");
                if (accept)
                {
                    signals_array[2] = "1";
                    uv_light1_box.BackgroundColor = Color.Green;
                }
                else
                {
                    switch_uv_1.IsToggled = false;
                }
            }
            else
            {
                signals_array[2] = "0";
                switch_uv_2.IsToggled = false;
                uv_light1_box.BackgroundColor = Color.Red;
            }
        }

        /// <summary>
        /// UV light 2nd level turn on/off event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Switch_uv_2_Toggled(object sender, ToggledEventArgs e)
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
                    signals_array[3] = "1";
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
                signals_array[3] = "0";
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
                UdpClient client = new UdpClient(get_signal_port);
                IPEndPoint ip = new IPEndPoint(robot_address, get_signal_port);

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        client.Close();
                        return;
                    }
                    signal_data = Encoding.UTF8.GetBytes(signals_array[0] + "|" + signals_array[1] + "|" + signals_array[2] + "|" + signals_array[3] + "|" + signals_array[4]);
                    client.Send(signal_data, signal_data.Length, ip);
                    //Debug.WriteLine("Was sent: " + signals_array[0] + "|" + signals_array[1] + "|" + signals_array[2] + "|" + signals_array[3] + "|" + signals_array[4]);
                    Thread.Sleep(10);
                }
            });
        }

        /// <summary>
        /// Gets view from robot/server
        /// </summary>
        private async void GetImage()
        {
            byte[] bytes = new byte[1024];
            await Task.Run(() =>
            {
                UdpClient client = new UdpClient(get_signal_port + 1);
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
        /// Gets information from Jetson (Arduino sensors)
        /// </summary>
        private async void GetStatusInfo()
        {
            byte[] bytes = new byte[64];
            await Task.Run(() =>
            {
                UdpClient client = new UdpClient(get_status_port);
                IPEndPoint ip = null;

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        client.Close();
                        return;
                    }

                    bytes = client.Receive(ref ip);
                    try
                    {
                        string[] status_message = Encoding.UTF8.GetString(bytes).Split('|');
                        line_track_status = status_message[0];
                        Debug.WriteLine("Track status is: " + line_track_status);
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    //line_track_status = status_message[0];
                    //TO DO...-----------------------try.. catch----------------------------------------------------------------------------------------
                    if (line_track_status[0] == '1')
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            line_status.BackgroundColor = Color.Black;
                        });
                    }
                    else if(line_track_status[0] == '0')
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            line_status.BackgroundColor = Color.Wheat;
                        });
                    }
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