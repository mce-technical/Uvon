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
        private int uv_port = 59472;
        private int speek_port = 55554;
        byte[] motor_data = new byte[1024];

        CancellationTokenSource motor_token_source, preview_token_source;
        CancellationToken motor_token, preview_token;

        private IPAddress robot_address;
        private IPAddress my_address;
        public ControlPanel(IPAddress my, IPAddress robot)
        {
            InitializeComponent();

            this.robot_address = robot;
            this.my_address = my;

            motor_data = Encoding.UTF8.GetBytes("no signal");

            motor_token_source = new CancellationTokenSource();
            preview_token_source = new CancellationTokenSource();
            motor_token = motor_token_source.Token;
            preview_token = preview_token_source.Token;
           
            SendSignal();

            Get_image();
        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            motor_data = Encoding.UTF8.GetBytes("01");
        }

        private void Right_Click(object sender, RoutedEventArgs e)
        {
            motor_data = Encoding.UTF8.GetBytes("04");
        }

        private void Left_Click(object sender, RoutedEventArgs e)
        {
            motor_data = Encoding.UTF8.GetBytes("03");

        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            motor_data = Encoding.UTF8.GetBytes("00");

        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            motor_data = Encoding.UTF8.GetBytes("02");
        }

        private void disconnect_Click(object sender, RoutedEventArgs e)
        {
            motor_data = Encoding.UTF8.GetBytes("34");

            Thread.Sleep(100);
            if (motor_token_source != null)
            {
                motor_token_source.Cancel();
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
            //TO DO..
        }

        /// <summary>
        /// Sends signals to robot/server
        /// </summary>
        private async void SendSignal()
        {
            await Task.Run(() =>
            {
                UdpClient client = new UdpClient(motor_port);
                IPEndPoint ip = new IPEndPoint(robot_address, motor_port);

                client.Send(motor_data, motor_data.Length, ip);

                while (true)
                {
                    if (motor_token.IsCancellationRequested)
                    {
                        client.Close();
                        return;
                    }
                    client.Send(motor_data, motor_data.Length, ip);
                    Debug.WriteLine("Was sent: " + motor_data[1]);
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
                UdpClient client = new UdpClient(image_port);
                IPEndPoint ip = null;

                while (true)
                {
                    if (preview_token.IsCancellationRequested)
                    {
                        client.Close();
                        return;
                    }
                    byte[] bytes = client.Receive(ref ip);
                    Debug.WriteLine(bytes.Length);

                    var image = ByteToImage(bytes);
                    image.Freeze();
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        imagesource.Source = image;
                    }));
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
    }
}
