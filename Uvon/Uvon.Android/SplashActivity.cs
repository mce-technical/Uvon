using Android.App;
using Android.Content.PM;
using Android.OS;

namespace Uvon.Droid
{
    [Activity(Theme = "@style/MyTheme.Splash",
        MainLauncher = true,
        NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            System.Threading.Thread.Sleep(2000);
            StartActivity(typeof(MainActivity));
        }
    }
}