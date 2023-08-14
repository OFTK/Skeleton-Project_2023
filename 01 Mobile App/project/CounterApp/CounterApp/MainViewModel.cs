using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Json;
using Newtonsoft.Json;
using System.Web;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Essentials;
using static Xamarin.Essentials.Permissions;
using XamarinEssentials = Xamarin.Essentials;
using System.Net.Mime;
using System.Text;

namespace CounterApp
{
    public static class ViewModelLocator
    {
        private static MainViewModel _myViewModel = new MainViewModel();
        public static MainViewModel MainViewModel
        {
            get
            {
                return _myViewModel;
            }
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {

        // fields
        /////////

        // connection and display
        public HubConnection connection;
        private static readonly string baseUrl          = "https://skeletonfunctionapp.azurewebsites.net";

        //private static readonly string baseUrl = "http://10.0.2.2:7071"; // this is the address to connect to the host
        private static readonly string getFamilyStatusUrl = baseUrl + "/api/getfamilystatus";
        private static readonly string updateBabyStatusUrl = baseUrl + "/api/updatebabystatus";

        public HttpClient client;

        // attributs to display baby status
        static string family = "family";

        public class FamilyStatus
        {
            public string family { get; set; }
            public List<Status> status { get; set; }
        }

        public class Status
        {
            public string babyname { get; set; }
            public string babyid { get; set; }
            public DateTime? lastupdate { get; set; }
            public double? latitude { get; set; }
            public double? longtitude { get; set; }
        }

        
        public class StatusUpdate
        {
            public string family { get; set; }
            public string babyname { get; set; }
            public string longtitude { get; set; }
            public string latitude { get; set; }
        }


        // TODO: use only one status type - this requires to change the API with the server...
        public class BabyStatus
        {
            public float? _BabyTemp = null;
            public float? _BabyHumd = null;
            public DateTime? _BabyLastSeenTime = null;
        }



        private FamilyStatus _familystatus;
        public FamilyStatus FamilyStatusDisplay
        {
            get => _familystatus;
            set => SetProperty(ref _familystatus, value);
        }

        // attributes to display messages from server
        private string _displayMessage;
        public string DisplayMessage 
        {
            get => _displayMessage; 
            set => SetProperty(ref _displayMessage, value);
        }

        private string _displayMessage2;
        public string DisplayMessage2
        {
            get => _displayMessage2;
            set => SetProperty(ref _displayMessage2, value);
        }


        private string device_ssid;
        public string DeviceSSID
        {
            get => device_ssid;
            set => SetProperty(ref device_ssid, value);
        }

        private string dev_connect_to_ssid;
        public string DevConnToSSID
        {
            get => dev_connect_to_ssid;
            set => SetProperty(ref dev_connect_to_ssid, value);
        }

        private string dev_connect_to_pass;
        public string DevConnToPass
        {
            get => dev_connect_to_pass;
            set => SetProperty(ref dev_connect_to_pass, value);
        }

        private byte[] aes_key;
        public byte[] AesKey
        {
            get => aes_key;
            set => SetProperty(ref aes_key, value);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // get baby status from the server
        //////////////////////////////////
        private void GetFamilyStatus()
        {
            // build http request query
            UriBuilder geturi_builder = new UriBuilder(getFamilyStatusUrl);
            var query = HttpUtility.ParseQueryString(geturi_builder.Query);
            query["family"] = family;
            geturi_builder.Query = query.ToString();

            // send http request
            HttpClient GetClient = new HttpClient();
            string url = geturi_builder.ToString();
            HttpResponseMessage resp = GetClient.GetAsync(url).Result;

            // display the response for debug purposes
            string response_string = resp.Content.ReadAsStringAsync().Result;
            DisplayMessage = response_string;


            // update status on screen
            FamilyStatus deserialize_family_status = JsonConvert.DeserializeObject<FamilyStatus>(response_string);
            FamilyStatusDisplay = deserialize_family_status;
        }

        private void StatusThread()
        {
            while (true)
            {
                GetFamilyStatus();
                Thread.Sleep(60000);
            }
        }

        // update nearby baby status to server
        //////////////////////////////////////

        private void UpdateServer(BabyStatus baby, Status curr_baby_status)
        {
            // put status from existing types in the type the server expects
            StatusUpdate babyupdate = new StatusUpdate();
            babyupdate.babyname = curr_baby_status.babyname;
            babyupdate.family = "family";
            babyupdate.longtitude = 32.3.ToString();
            babyupdate.latitude = 32.3.ToString();

            // send http request
            HttpClient PostClient = new HttpClient();
            HttpResponseMessage resp = PostClient.PostAsync(
                updateBabyStatusUrl,
                // serialize babyupdate
                new StringContent(
                    JsonConvert.SerializeObject(babyupdate), Encoding.UTF8, "application/json")
                ).Result;
        }

        public void UpdateThread()
        {

            while (true)
            {
                var scanner = new BLEScanner();

                for (int i = 0; i < FamilyStatusDisplay.status.Count; i++)
                {
                    Status baby = FamilyStatusDisplay.status[i];
                    BabyStatus result = scanner.BLEScan(baby.babyid, aes_key, dev_connect_to_ssid, dev_connect_to_pass).Result;

                    if (result != null && result._BabyTemp != null)
                    {
                        DisplayMessage = "Temperature: " + result._BabyTemp.ToString() + "\nHumidity: " + result._BabyHumd.ToString() + "\n, Time: " + result._BabyLastSeenTime.ToString();

                        if (!scanner.dev_got_wifi_creds) // If the device has creds, he does it himself
                        {
                            Console.WriteLine("Device got no wifi, updating server...");
                            UpdateServer(result, baby);
                        } else
                        {
                            DeviceSSID = scanner.dev_wifi_ssid;
                            Console.WriteLine("Device got wifi with ssid: " + DeviceSSID);
                        }
                    } else
                    {
                        DeviceSSID = null;
                    }
                }

                Thread.Sleep(1000);
            }
        }

        // signalr tasks
        ////////////////
        private async Task ConnectToSignalr()
        {
            // on alert - activate alert flow
            connection.On<string>("counterUpdate", (data) =>
            {
                // TODO: add alert flow
            });
            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
            await connection.StartAsync();
        }

        // to send messages to signalr do:
        // connection.InvokeAsync("asdf", "asdf").Result

        // MainViewModel
        ////////////////
        public MainViewModel()
        {
            // Setting ble settings...
            dev_connect_to_ssid = null;
            dev_connect_to_pass = null;
            device_ssid = null;

            // init objects for communication
            client = new HttpClient();
            connection = new HubConnectionBuilder().WithUrl(new Uri(baseUrl + "/api")).Build();
            Task signalr_connection_task = Task.Run(async () => await ConnectToSignalr());
            FamilyStatusDisplay = new FamilyStatus();
            GetFamilyStatus();
            Thread update_family_status_thread = new Thread(StatusThread){};
            update_family_status_thread.Start();
            Thread update_baby_status_thread = new Thread(UpdateThread) { };
            update_baby_status_thread.Start();
            client.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}