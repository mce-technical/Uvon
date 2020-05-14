using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        private int speek_port = 55554;

        private string[] signal = new string[2];     //first is motor, second is uv

        CancellationTokenSource signal_token_source, preview_token_source;
        CancellationToken signal_token, preview_token;

        private IPAddress robot_address;
        private IPAddress my_address;
        public ControlPanel(IPAddress my, IPAddress robot)
        {
            InitializeComponent();

            signal[0] = "00";
            signal[1] = "00";

            this.robot_address = robot;
            this.my_address = my;

            signal_token_source = new CancellationTokenSource();
            preview_token_source = new CancellationTokenSource();
            signal_token = signal_token_source.Token;
            preview_token = preview_token_source.Token;
           
            //SendSignal();

            Get_image();
        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            signal[0] = "01";
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            signal[0] = "04";
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            signal[0] = "03";
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            signal[0] = "00";
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            signal[0] = "02";
        }

        private void disconnect_Click(object sender, RoutedEventArgs e)
        {
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

        private void uv_Click(object sender, RoutedEventArgs e)
        {
            if(uv.Content.ToString() == "Turn On UV")
            {
                MessageBoxResult result = MessageBox.Show("You are going to enable UV light", "UV", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        signal[1] = "01";
                        uv.Content = "Turn Off UV";
                        break;
                    case MessageBoxResult.No:
                        signal[1] = "00";
                        break;
                }
            }
            else if(uv.Content.ToString() == "Turn Off UV")
            {
                signal[1] = "00";
                uv.Content = "Turn On UV";
            }
        }

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

                signal_bytes = Encoding.UTF8.GetBytes(signal[0] + "|" + signal[1]);
                client.Send(signal_bytes, signal_bytes.Length, ip);

                while (true)
                {
                    if (signal_token.IsCancellationRequested)
                    {
                        client.Close();
                        return;
                    }
                    signal_bytes = Encoding.UTF8.GetBytes(signal[0] + "|" + signal[1]);
                    client.Send(signal_bytes, signal_bytes.Length, ip);
                    Debug.WriteLine("Was sent: " + signal[0] + " " + signal[1]);

                    Thread.Sleep(100);
                }
            });
        }


        /// <summary>
        /// Gets image from Python server and sets it on image source box
        /// </summary>
        async void Get_image()
        {
            byte[] bytes = new byte[1024];

            await Task.Run(() =>
            {
                UdpClient client = new UdpClient(image_port);
                IPEndPoint ip = null;

                while (true)
                {
                    if (preview_token.IsCancellationRequested)
                    {
                        //client.Close();
                        break;
                    }
                    bytes = client.Receive(ref ip);

                    var image = ByteToImage(bytes);
                    image.Freeze();
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        imagesource.Source = image;
                    }));
                }
                client.Close();
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
    }
}
