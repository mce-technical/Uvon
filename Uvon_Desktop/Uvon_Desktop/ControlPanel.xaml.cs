using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Uvon_Desktop
{
    /// <summary>
    /// Interaction logic for ControlPanel.xaml
    /// </summary>
    public partial class ControlPanel : Window
    {
        #region Page fields

        private ushort image_port = 55556;                         // The port to get image frames from server/robot
        private ushort motor_port = 55555;                         // The port to send motor controlling signls to robot
        private ushort get_status_port = 53784;                    // The port to get info about robot's state 

        private string online_satus = "0",                         // The robot's components status
            battery_1_status = "0", 
            battery_2_status = "0",
            motor_driver_status = "0",
            uv1_status = "0",
            uv2_status = "0";

        private string[] signal = new string[6];                   // signalses array to send the signals as one to Jetson
        bool noconnection;                                         // Keeps the connection state of preview
        bool disconneted;                                          // Keeps the connection state of motor control

        UvonInfo infoPage;                                         // information page for users

        CancellationTokenSource signal_token_source;               // To cancel controlling Task
        CancellationTokenSource preview_token_source;              // To cancel preview Task
        CancellationToken signal_token;
        CancellationToken preview_token;

        private IPAddress robot_address;                           // Ip address of robot/server

        SolidColorBrush motor_brush = new SolidColorBrush();       // UI colors
        SolidColorBrush uv1_brush = new SolidColorBrush();
        SolidColorBrush uv2_brush = new SolidColorBrush();
        SolidColorBrush autopilot_brush = new SolidColorBrush();
        SolidColorBrush line_status_brush = new SolidColorBrush();

        #endregion
        public ControlPanel(IPAddress my, IPAddress robot)
        {
            InitializeComponent();

            signal[0] = "00";
            signal[1] = "00";
            signal[2] = "0";
            signal[3] = "0";
            signal[4] = "0";
            signal[5] = "0";

            noconnection = false;
            disconneted = false;

            this.robot_address = robot;

            signal_token_source = new CancellationTokenSource();
            preview_token_source = new CancellationTokenSource();
            signal_token = signal_token_source.Token;
            preview_token = preview_token_source.Token;

            SendSignal();

            Get_image();

            GetStatusInfo();

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                info_image.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\info.png", UriKind.Absolute));
            }));

            Task.Run(() =>
            {
                Thread.Sleep(1000);
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    noconnection = imagesource.Source == null;
                }));
                Thread.Sleep(3000);

                if (noconnection)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        imagesource.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\noconnection.png", UriKind.Absolute));
                    }));
                }
            });

            Task.Run(() =>
            {
                UpdateStates();
            });
        }

        #region Control Buttons

        private void Forward()
        {        
            signal[0] = "1";
        }

        private void Backward()
        {
            signal[0] = "2";
        }

        private void TurnLeft()
        {
            signal[0] = "3";
        }

        private void TurnRight()
        {
            signal[0] = "4";
        }

        /// <summary>
        /// Sets robot to autopilot. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Autopilot_button_Click(object sender, RoutedEventArgs e)
        {
            if (Autopilot_button.Content.ToString() == "Line tracking")
            {
                signal[4] = "1";
                autopilot_brush.Color = Colors.Green;
                Autopilot_button.Content = "Hand Control";
            }
            else if (Autopilot_button.Content.ToString() == "Hand Control")
            {
                signal[4] = "0";
                autopilot_brush.Color = Colors.Red;
                Autopilot_button.Content = "Line tracking";
            }
            autopilot.Background = autopilot_brush;
        }

        /// <summary>
        /// Opens a window with a simple instructions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Info_Click(object sender, RoutedEventArgs e)
        {
            if (infoPage == null)
            {
                infoPage = new UvonInfo();
                infoPage.Closed += delegate { infoPage = null; };
                infoPage.Show();
            }
            else
            {
                infoPage.WindowState = WindowState.Normal;
                infoPage.Activate();
            }
        }

        /// <summary>
        /// This method enables or disables Arduino system
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void On_Off_Click(object sender, RoutedEventArgs e)
        {
            if (Enable.Content.ToString() == "ON")
            {
                signal[1] = "ON";
                Enable.Content = "OFF";
                //motor_brush.Color = Colors.Green;
                //motor_drivers.Background = motor_brush;
            }
            else if (Enable.Content.ToString() == "OFF")
            {
                signal[1] = "OFF";
                Enable.Content = "ON";
                //motor_brush.Color = Colors.Red;
                //motor_drivers.Background = motor_brush;
            }
        }

        /// <summary>
        /// To switch camera 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CameraSwitch_Click(object sender, RoutedEventArgs e)
        {
            if(camera_switch.Content.ToString()=="Camera 2")
            {
                signal[5] = "1";
                camera_switch.Content = "Camera 1";
            }
            else if (camera_switch.Content.ToString() == "Camera 1")
            {
                signal[5] = "0";
                camera_switch.Content = "Camera 2";
            }
        }

        /// <summary>
        /// To disconnect client from server/robot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            signal[2] = "0";
            signal[3] = "0";
            if (signal[1] == "ON")
            {
                signal[1] = "OFF";
            }
            disconneted = true;
            signal[0] = "34";

            Thread.Sleep(100);
            if (signal_token_source != null)
            {
                signal_token_source.Cancel();
            }
            if (preview_token_source != null)
            {
                preview_token_source.Cancel();
            }

            MainWindow main = new MainWindow();
            main.Show();

            this.Close();
        }

        /// <summary>
        /// When user clicks on this button, it opens UV light's 1st level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Uv1_Click(object sender, RoutedEventArgs e)
        {
            if (uv1.Content.ToString() == "UV 1 Enable")
            {
                MessageBoxResult result = MessageBox.Show("You are going to enable UV lamp", "UV 1", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        signal[2] = "1";
                        uv1.Content = "UV 1 Disable";
                        //uv1_brush.Color = Colors.Green;
                        break;
                    case MessageBoxResult.No:
                        signal[2] = "0";
                        //uv1_brush.Color = Colors.Red;
                        break;
                }
            }
            else if (uv1.Content.ToString() == "UV 1 Disable")
            {
                signal[2] = "0";
                uv1.Content = "UV 1 Enable";
                //uv1_brush.Color = Colors.Red;
            }
            //uv_light_1.Background = uv1_brush;
        }

        /// <summary>
        /// When user clicks on this button, it opens UV light's 2nd level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Uv2_Click(object sender, RoutedEventArgs e)
        {
            if (uv2.Content.ToString() == "UV 2 Enable")
            {
                MessageBoxResult result = MessageBox.Show("You are going to enable UV lamp", "UV 2", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        signal[3] = "1";
                        uv2.Content = "UV 2 Disable";
                        //uv2_brush.Color = Colors.Green;
                        break;
                    case MessageBoxResult.No:
                        signal[3] = "0";
                        //uv2_brush.Color = Colors.Red;
                        break;
                }
            }
            else if (uv2.Content.ToString() == "UV 2 Disable")
            {
                signal[3] = "0";
                uv2.Content = "UV 2 Enable";
                //uv2_brush.Color = Colors.Red;
            }
            //uv_light_2.Background = uv2_brush;
        }

        #endregion 


        /// <summary>
        /// Sends signals to robot/server
        /// </summary>
        private async void SendSignal()
        {
            byte[] signal_bytes;
            await Task.Run(() =>
            {
                UdpClient client = new UdpClient(motor_port);
                IPEndPoint ip = new IPEndPoint(robot_address, motor_port);

                while (true)
                {
                    if (signal_token.IsCancellationRequested)
                    {
                        client.Close();
                        Debug.WriteLine("Connection signal is over");
                        return;
                    }
                    signal_bytes = Encoding.UTF8.GetBytes(signal[0] + "|" + signal[1] + "|" + signal[2] + "|" + signal[3] + "|" + signal[4] + "|" + signal[5]);
                    client.Send(signal_bytes, signal_bytes.Length, ip);
                    Debug.WriteLine("Was sent: " + signal[0] + " " + signal[1] + "|" + signal[2] + "|" + signal[3] + "|" + signal[4] + "|" + signal[5]);

                    Thread.Sleep(100);
                }
            });
        }


        /// <summary>
        /// Gets image from Python server and sets it on image source box
        /// </summary>
        async void Get_image()
        {
            await Task.Run(() =>
            {
                try
                {
                    UdpClient client = new UdpClient(image_port);
                    IPEndPoint ip = new IPEndPoint(robot_address, 0);

                    while (true)
                    {
                        var bytes = client.Receive(ref ip);
                        var image = ByteToImage(bytes);

                        image.Freeze();

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            imagesource.Source = image;
                        }));

                        if (preview_token.IsCancellationRequested)
                        {
                            Debug.WriteLine("Preview is canceled");
                            break;
                        }
                    }
                    Thread.Sleep(500);
                    client.Client.Dispose();
                    client.Client.Close();
                    client.Close();
                    GC.Collect();
                    Debug.WriteLine("Connection is end");
                }
                catch (Exception exp)
                {
                    Debug.WriteLine(exp.Message);
                }
            }, preview_token);
        }

        /// <summary>
        /// Gets information from Jetson (Arduino sensors)
        /// </summary>
        private async void GetStatusInfo()
        {
            byte[] bytes = new byte[64];
            await Task.Run(() =>
            {
                try
                {
                    UdpClient client = new UdpClient(get_status_port);
                    IPEndPoint ip = null;

                    while (true)
                    {
                        if (signal_token.IsCancellationRequested)
                        {
                            client.Client.Close();
                            client.Close();
                            return;
                        }

                        bytes = client.Receive(ref ip);
                        try
                        {
                            var status_message = Encoding.UTF8.GetString(bytes).Split('|');
                            online_satus = status_message[0];
                            motor_driver_status = status_message[1];
                            uv1_status = status_message[2];
                            uv2_status = status_message[3];
                            battery_1_status = status_message[4];
                            battery_2_status = status_message[5];
                            Debug.WriteLine(Encoding.UTF8.GetString(bytes));
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }

                        if (online_satus[0] == '1')
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                line_status_brush.Color = Colors.Black;
                                online_status.Background = line_status_brush;
                            }));
                        }
                        else if (online_satus[0] == '0')
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                line_status_brush.Color = Colors.Wheat;
                                online_status.Background = line_status_brush;
                            }));
                        }   
                    }
                }
                catch(Exception exp)
                {
                    Debug.WriteLine(exp.Message);
                }
            });
        }


        /// <summary>
        /// This method reads image from byte's array
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        static ImageSource ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();

            ImageSource imgSrc = biImg as ImageSource;

            return imgSrc;
        }


        /// <summary>
        /// Binding buttons to keyboard's keys
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    Forward();
                    break;
                case Key.Down:
                    Backward();
                    break;
                case Key.Left:
                    TurnLeft();
                    break;
                case Key.Right:
                    TurnRight();
                    break;
                case Key.RightShift:
                    On_Off_Click(new object(), new RoutedEventArgs());
                    break;
            }
        }

        /// <summary>
        /// When there is no key that is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (disconneted)
            {
                signal[0] = "34";
            }
            else
            {
                signal[0] = "0";
            }
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
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            motor_brush.Color = Colors.Red;
                            motor_drivers.Background = motor_brush;
                        }));
                    }
                    else if (motor_driver_status == "1")
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            motor_brush.Color = Colors.Green;
                            motor_drivers.Background = motor_brush;
                        }));
                    }
                    motor_previous_state = motor_driver_status;
                }

                //Checking if the UV1 status is changed
                if (uv1_status != uv1_previous_state)
                {
                    if(uv1_status == "I1")
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            uv1_brush.Color = Colors.Green;
                            uv_light_1.Background = uv1_brush;
                        }));
                    }
                    else if(uv1_status == "I0")
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            uv1_brush.Color = Colors.Red;
                            uv_light_1.Background = uv1_brush;
                        }));
                    }

                    uv1_previous_state = uv1_status;
                }

                //Checking if the UV2 status is changed
                if (uv2_status != uv2_previous_state)
                {
                    if (uv2_status == "U1")
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            uv2_brush.Color = Colors.Green;
                            uv_light_2.Background = uv2_brush;
                        }));
                    }
                    else if (uv2_status == "U0")
                    {
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            uv2_brush.Color = Colors.Red;
                            uv_light_2.Background = uv2_brush;
                        }));
                    }

                    uv2_previous_state = uv2_status;
                }

                //Checking if the battery 1 state is changed
                if(battery_1_status != battery1_previous_state)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        battery_1.Value = double.Parse(battery_1_status);
                    }));
                    battery1_previous_state = battery_1_status;
                }

                //Checking if the battery 2 state is changed
                if (battery_2_status != battery2_previous_state)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        battery_2.Value = double.Parse(battery_2_status);                      
                    }));
                    battery2_previous_state = battery_2_status;
                }

                Thread.Sleep(250);
            }
        }


        /// <summary>
        /// Closes all connection after clicking on close button
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            signal[0] = "34";

            if(signal[1] == "ON")
            {
                signal[1] = "OFF";
            }
            signal[2] = "0";
            signal[3] = "0";
            Thread.Sleep(100);
            if (signal_token_source != null)
            {
                signal_token_source.Cancel();
            }
            if (preview_token_source != null)
            {
                preview_token_source.Cancel();
            }

            if (infoPage != null)
                infoPage.Close();
        }

    }
}