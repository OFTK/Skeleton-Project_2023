using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using CounterApp;

namespace CounterApp.Droid
{
    [Activity(Label = "ilovemybaby", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Xamarin.Essentials.Platform.Init(this, bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            LoadApplication(new App());

            MainThread.BeginInvokeOnMainThread(async () => await Permissions.RequestAsync<BLEPermission>());
        }
        // protected override async void OnCreate(Bundle savedInstanceState)
        // {
        //     base.OnCreate(savedInstanceState);

        //     Xamarin.Essentials.Platform.Init(this, savedInstanceState);
        //     global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
        //     LoadApplication(new App());

        //     await Permissions.RequestAsync<BLEPermission>();
        // }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        // Function below is required to ask for permission to use Bluetooth
        public class BLEPermission : Xamarin.Essentials.Permissions.BasePlatformPermission
        {
            public override (string androidPermission, bool isRuntime)[] RequiredPermissions => new List<(string androidPermission, bool isRuntime)>
{
                (Android.Manifest.Permission.BluetoothScan, true),
                (Android.Manifest.Permission.BluetoothConnect, true)
            }.ToArray();
        }
    }
}