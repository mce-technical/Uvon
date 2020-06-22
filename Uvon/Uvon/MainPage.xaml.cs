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
        #region Page fields

        private ushort get_signal_port = 55555;             // Port to get connected with Jetson
        private ushort get_image_port = 55556;
        private ushort get_status_port = 53784;

        private string[] signals_array = new string[6];     // Signals array to send to Jetson
        private string online_status = "0",                 // Info about robot's components
            battery_1_status = "0",
            battery_2_status = "0",
            motor_driver_status = "0",
            uv1_status = "0",
            uv2_status = "0";

        double page_width;
        double element_size;

        IPAddress robot_address;                            //Robot/Server IPv4 address

        CancellationTokenSource cancelConnections;          //Cancelation token source and token to terminate other tasks
        CancellationToken token;

        #endregion

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

            signals_array[0] = "00";                   //Motor's signal
            signals_array[1] = "00";                   //Motor drivers ON/OFF signal
            signals_array[2] = "0";                    //UV 1 signal
            signals_array[3] = "0";                    //UV 2 signal
            signals_array[4] = "0";                    //Line tracking mode on/off   
            signals_array[5] = "0";                    //Camera switch signal            

            preview.Source = im;

            GetImage();    //Starting a new task to get preview from server/robot

            SendSignal();   //Starting a new task to send signal to control motors of robor.

            GetStatusInfo();    //Starting a new task to get information about robot's components

            Task.Run(() =>      //Starting a new task to update information on UI
            {
                UpdateStates();     
            });
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
            if (signals_array[1] == "ON")
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
            if (turn.Text == "Turn On")
            {
                signals_array[1] = "ON";
                turn.Text = "Turn Off";
                //motors_box.BackgroundColor = Color.Green;
            }
            else if (turn.Text == "Turn Off")
            {
                signals_array[1] = "OFF";
                turn.Text = "Turn On";
                //motors_box.BackgroundColor = Color.Red;
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
        /// To switch camera
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CameraSwitch_Clicked(object sender, EventArgs e)
        {
            if (camera_switch.Text == "Camera 2")
            {
                signals_array[5] = "1";
                camera_switch.Text = "Camera 1";
            }
            else if (camera_switch.Text == "Camera 1")
            {
                signals_array[5] = "0";
                camera_switch.Text = "Camera 2";
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

            if (signals_array[1] == "ON")
            {
                signals_array[1] = "OFF";
            }

            Thread.Sleep(100);

            if (cancelConnections != null)
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
                    //uv_light1_box.BackgroundColor = Color.Green;
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
                //uv_light1_box.BackgroundColor = Color.Red;
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
                    //uv_light2_box.BackgroundColor = Color.Green;
                }
                else
                {
                    switch_uv_2.IsToggled = false;
                }
            }
            else
            {
                //uv_light2_box.BackgroundColor = Color.Red;
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
                    signal_data = Encoding.UTF8.GetBytes(signals_array[0] + "|" + signals_array[1] + "|" + signals_array[2] + "|" + signals_array[3] + "|" + signals_array[4] + "|" + signals_array[5]);
                    client.Send(signal_data, signal_data.Length, ip);
                    //Debug.WriteLine("Was sent: " + signals_array[0] + "|" + signals_array[1] + "|" + signals_array[2] + "|" + signals_array[3] + "|" + signals_array[4] + "|" + signals_array[5]);
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
                UdpClient client = new UdpClient(get_image_port);
                IPEndPoint ip = null;

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        client.Close();
                        return;
                    }
                    Thread.Sleep(120);
                    bytes = client.Receive(ref ip);
                    Debug.WriteLine(bytes.Length);
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
                UdpClient client = new UdpClient(get_status_port);      //object for UDP connections
                IPEndPoint ip = null;                                   //IP endpoint of the server

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        client.Close();
                        return;
                    }

                    //Listens incoming bytes
                    bytes = client.Receive(ref ip);

                    try
                    {
                        string[] status_message = Encoding.UTF8.GetString(bytes).Split('|');
                        online_status = status_message[0];
                        motor_driver_status = status_message[1];
                        uv1_status = status_message[2];
                        uv2_status = status_message[3];
                        battery_1_status = status_message[4];
                        battery_2_status = status_message[5];

                        Debug.WriteLine(status_message.ToString());
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                    if (online_status[0] == '1')
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            line_status.BackgroundColor = Color.Black;
                        });
                    }
                    else if (online_status[0] == '0')
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


        /// <summary>
        /// Updates UI elements according to the Robot's state
        /// </summary>
        private void UpdateStates()
        {
            //In those variables we keep the previous state signal from Jetson
            string motor_previous_state = "0",
                uv1_previous_state = "0", 
                uv2_previous_state = "0", 
                battery1_previous_state = "0",
                battery2_previous_state = "0";

            while (true)
            {
                //Checking if the M. D. status is changed
                if (motor_driver_status != motor_previous_state)
                {
                    if (motor_driver_status == "2")
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            motors_box.BackgroundColor = Color.Red;
                        });
                    }
                    else if (motor_driver_status == "1")
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            motors_box.BackgroundColor = Color.Green;
                        });
                    }
                    motor_previous_state = motor_driver_status;
                }

                //Checking if the UV1 status is changed
                if (uv1_status != uv1_previous_state)
                {
                    if (uv1_status == "I1")
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            uv_light1_box.BackgroundColor = Color.Green;
                        });
                    }
                    else if (uv1_status == "I0")
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            uv_light1_box.BackgroundColor = Color.Red;
                        });
                    }

                    uv1_previous_state = uv1_status;
                }

                //Checking if the UV2 status is changed
                if (uv2_status != uv2_previous_state)
                {
                    if (uv2_status == "U1")
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            uv_light2_box.BackgroundColor = Color.Green;
                        });
                    }
                    else if (uv2_status == "U0")
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            uv_light2_box.BackgroundColor = Color.Red;
                        });
                    }

                    uv2_previous_state = uv2_status;
                }

                //Checking if the battery 1 state is changed
                if (battery_1_status != battery1_previous_state)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        battery_1.Progress = double.Parse(battery_1_status) / 100;
                    });

                    battery1_previous_state = battery_1_status;
                }

                //Checking if the battery 2 state is changed
                if (battery_2_status!= battery2_previous_state)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        battery_2.Progress = double.Parse(battery_2_status) / 100;
                    });
                    battery2_previous_state = battery_2_status;
                }

                Thread.Sleep(250);
            }
        }
    }
}