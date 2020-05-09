using System;
using Uvon.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Uvon
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MyMenu();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
