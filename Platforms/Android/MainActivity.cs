using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Mercurio.Driver.Services; 

namespace Mercurio.Driver
{
    [Preserve(AllMembers = true)]
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        private GpsServiceConnection _gpsServiceConnection;

        // Static property to access the service from MAUI
        public static IGpsService GpsService { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Start and bind the service
            StartAndBindGpsService();
            
        }

        private void StartAndBindGpsService()
        {
            var serviceIntent = new Intent(this, typeof(GpsServiceAndroid));

            // Simply start the service. It's not forced to the foreground yet.
            // Binding (BindService) will keep it alive as long as the app is open.
            StartService(serviceIntent);

            // Start the service so it can run indefinitely
            // StartForegroundService is required for API 26+
            /*if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                StartForegroundService(serviceIntent);
            }
            else
            {
                StartService(serviceIntent);
            }*/

            // Link the service so we can communicate with it
            _gpsServiceConnection = new GpsServiceConnection();
            BindService(serviceIntent, _gpsServiceConnection, Bind.AutoCreate);
        }

        // Class to manage the connection with the service
        [Preserve(AllMembers = true)]
        private class GpsServiceConnection : Java.Lang.Object, IServiceConnection
        {
            public void OnServiceConnected(ComponentName name, IBinder service)
            {
                if (service is GpsServiceBinder binder)
                {
                    // We make the service instance available to the entire app
                    MainActivity.GpsService = binder.Service;
                }
            }

            public void OnServiceDisconnected(ComponentName name)
            {
                MainActivity.GpsService = null;
            }
        }
    }
}


/*namespace Mercurio.Driver
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}*/
