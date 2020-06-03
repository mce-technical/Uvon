﻿using System;
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
        private int image_port = 55556;
        private int motor_port = 55555;

        private string[] signal = new string[3];     //first is motor, second is uv
        bool noconnection;

        UvonInfo infoPage;

        CancellationTokenSource signal_token_source;
        CancellationTokenSource preview_token_source;
        CancellationTokenSource key_release_source;
        CancellationToken signal_token;
        CancellationToken preview_token;
        CancellationToken key_token;

        private IPAddress robot_address;
        private IPAddress my_address;

        SolidColorBrush motor_brush = new SolidColorBrush();
        SolidColorBrush uv1_brush = new SolidColorBrush();
        SolidColorBrush uv2_brush = new SolidColorBrush();
        SolidColorBrush autopilot_brush = new SolidColorBrush();

        public ControlPanel(IPAddress my, IPAddress robot)
        {
            InitializeComponent();

            signal[0] = "00";
            signal[1] = "00";
            signal[2] = "0";

            noconnection = false;

            this.robot_address = robot;
            this.my_address = my;

            signal_token_source = new CancellationTokenSource();
            preview_token_source = new CancellationTokenSource();
            key_release_source = new CancellationTokenSource();
            signal_token = signal_token_source.Token;
            preview_token = preview_token_source.Token;
            key_token = key_release_source.Token;

            SendSignal();

            Get_image();

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                info_image.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\info.png", UriKind.Absolute));
            }));

            Task.Run(() =>
            {
                Thread.Sleep(3000);
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    noconnection = imagesource.Source == null;
                }));

                if (noconnection)
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        imagesource.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\noconnection.png", UriKind.Absolute));
                    }));
                }
            });

            var thread = new Thread(new ThreadStart(Release));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        #region Control Buttons

        void Release()
        {
            while (!key_token.IsCancellationRequested)
            {
                if (Keyboard.IsKeyUp(Key.Up) && Keyboard.IsKeyUp(Key.Down) && Keyboard.IsKeyUp(Key.Left) && Keyboard.IsKeyUp(Key.Right))
                {
                    signal[0] = "0";
                }
            }
            signal[0] = "34";
        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {        
            signal[0] = "1";
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            signal[0] = "2";
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            signal[0] = "3";
        }

        private void Right_Click(object sender, RoutedEventArgs e)
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
            if (Autopilot_button.Content.ToString() == "Autopilot")
            {
                autopilot_brush.Color = Colors.Green;
                Autopilot_button.Content = "Hand Control";
            }
            else if (Autopilot_button.Content.ToString() == "Hand Control")
            {
                autopilot_brush.Color = Colors.Red;
                Autopilot_button.Content = "Autopilot";
            }
            autopilot.Background = autopilot_brush;
            //TO DO...
            //If autopilot is ON disable hand control buttons
        }

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
                signal[2] = "ON";
                Enable.Content = "OFF";
                motor_brush.Color = Colors.Green;
                motor_drivers.Background = motor_brush;
            }
            else if (Enable.Content.ToString() == "OFF")
            {
                signal[2] = "OFF";
                Enable.Content = "ON";
                motor_brush.Color = Colors.Red;
                motor_drivers.Background = motor_brush;
            }
        }

        /// <summary>
        /// To disconnect client from server/robot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void disconnect_Click(object sender, RoutedEventArgs e)
        {
            signal[1] = "00";
            signal[2] = "OFF";

            if (key_release_source != null)
            {
                key_release_source.Cancel();
            }
            Thread.Sleep(100);
            if (signal_token_source != null)
            {
                signal_token_source.Cancel();
            }
            if (preview_token_source != null)
            {
                preview_token_source.Cancel();
            }
            
            Thread.Sleep(100);
            signal[0] = "34";

            MainWindow main = new MainWindow();
            main.Show();

            this.Close();
        }

        /// <summary>
        /// When user clicks on this button, it opens UV light's 1st level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uv1_Click(object sender, RoutedEventArgs e)
        {
            if (uv1.Content.ToString() == "UV Level 1 Enable")
            {
                MessageBoxResult result = MessageBox.Show("You are going to enable UV light", "UV", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        signal[1] = "01";
                        uv1.Content = "UV Level 1 Disable";
                        uv1_brush.Color = Colors.Green;
                        break;
                    case MessageBoxResult.No:
                        signal[1] = "00";
                        uv1_brush.Color = Colors.Red;
                        break;
                }
            }
            else if (uv1.Content.ToString() == "UV Level 1 Disable")
            {
                signal[1] = "00";
                uv1.Content = "UV Level 1 Enable";
                uv1_brush.Color = Colors.Red;
            }
            uv_light_1.Background = uv1_brush;
        }

        /// <summary>
        /// When user clicks on this button, it opens UV light's 2nd level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void uv2_Click(object sender, RoutedEventArgs e)
        {
            if (uv2.Content.ToString() == "UV Level 2 Enable")
            {
                MessageBoxResult result = MessageBox.Show("You are going to enable UV light", "UV", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        //signal[1] = "01";
                        uv2.Content = "UV Level 2 Disable";
                        uv2_brush.Color = Colors.Green;
                        break;
                    case MessageBoxResult.No:
                        //signal[1] = "00";
                        uv2_brush.Color = Colors.Red;
                        break;
                }
            }
            else if (uv2.Content.ToString() == "UV Level 2 Disable")
            {
                signal[1] = "00";
                uv2.Content = "UV Level 2 Enable";
                uv2_brush.Color = Colors.Red;
            }
            uv_light_2.Background = uv2_brush;
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

                signal_bytes = Encoding.UTF8.GetBytes(signal[0] + "|" + signal[1] + "|" + signal[2]);
                client.Send(signal_bytes, signal_bytes.Length, ip);

                while (true)
                {
                    if (signal_token.IsCancellationRequested)
                    {
                        client.Close();
                        Debug.WriteLine("Connection signal is over");
                        return;
                    }
                    signal_bytes = Encoding.UTF8.GetBytes(signal[0] + "|" + signal[1] + "|" + signal[2]);
                    client.Send(signal_bytes, signal_bytes.Length, ip);
                    Debug.WriteLine("Was sent: " + signal[0] + " " + signal[1] + " " + signal[2]);

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
                    Thread.Sleep(900);
                    client.Client.Dispose();
                    client.Client.Close();
                    GC.Collect();
                    Debug.WriteLine("Connection is end");
                }
                catch (Exception e)
                {

                }
            }, preview_token);
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
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    Go_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.Down:
                    Back_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.Left:
                    Left_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.Right:
                    Right_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.RightShift:
                    On_Off_Click(new object(), new RoutedEventArgs());
                    break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {

        }

        /// <summary>
        /// Closes all connection after clicking on close button
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            signal[0] = "34";
            signal[1] = "00";

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
