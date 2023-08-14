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
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using CounterApp.Services;

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
        public string _ViewModelNotStarted = "true";
        public string ViewModelNotStarted { 
            get => _ViewModelNotStarted;
            set => SetProperty(ref _ViewModelNotStarted, value);
        }
        public ICommand StartViewModelCommand { get; }

        // connection and display
        public HubConnection connection;
        // private static readonly string baseUrl = "https://skeletonfunctionapp.azurewebsites.net";
        private static readonly string baseUrl = "https://ilovemybabysecure.azurewebsites.net";
        private static readonly string getFamilyDetailsUrl = baseUrl + "/api/getfamilystatus";
        private static readonly string updateBabyDetailsUrl = baseUrl + "/api/updatebabystatus";

        public HttpClient client;

        // attributs to display baby details
        static string family = "family";
        
        public class BabyDetails
        {
            public string babyname { get; set; }
            public string babyid { get; set; }
            public string displaystring { get; set; }
            public bool baby_is_ok { get; set; }
            public DateTime? lastupdate { get; set; }
            public string location { get; set; }
            public float? temperature { get; set; }
            public float? humidity { get; set; }
            
        }
        public class FamilyDetails
        {
            public string family { get; set; }
            public List<BabyDetails> details { get; set; }
        }
        public FamilyDetails _localFamilyDetails;
        public FamilyDetails LocalFamilyDetails { 
            get => _localFamilyDetails;
            set => SetProperty(ref _localFamilyDetails, value);
        }
        
        public class DetailsUpdate
        {
            public string family { get; set; }
            public string babyname { get; set; }
            public string longtitude { get; set; }
            public string latitude { get; set; }
        }

        public class BabyStatus
        {
            public float? _BabyTemp = null;
            public float? _BabyHumd = null;
            public DateTime? _BabyLastSeenTime = null;
        }

        // alerts from signalr
        public class BabyAlert
        {
            public string babyname { get; set; }
            public string alertreason { get; set; }
        }
        public List<BabyAlert> AlertList { get; set; }


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

        // get baby details from the server
        //////////////////////////////////
        private void GetFamilyDetails()
        {
            // build http request query
            UriBuilder geturi_builder = new UriBuilder(getFamilyDetailsUrl);
            var query = HttpUtility.ParseQueryString(geturi_builder.Query);
            query["family"] = family;
            geturi_builder.Query = query.ToString();

            // send http request
            // HttpClient GetClient = new HttpClient();
            // string url = geturi_builder.ToString();
            // HttpResponseMessage resp = GetClient.GetAsync(url).Result;

            var azureService = DependencyService.Get<IAzureService>();
            string response_string = azureService.GetFamilyDetailsFromServer().Result;

            // display the response for debug purposes
            // string response_string = resp.Content.ReadAsStringAsync().Result;
            Debug.WriteLine(response_string);
            Debug.WriteLine(connection.ConnectionId);

            
            FamilyDetails updatedFamilyDetails = new FamilyDetails();
            updatedFamilyDetails.details = new List<BabyDetails>();
            // parse response
            JObject json = JObject.Parse(response_string);
            updatedFamilyDetails.family = (string)json["family"];
            JArray items = (JArray)json["status"];
            // put each field in item into a new BabyDetails object
            foreach (JObject item in items)
            {
                BabyDetails newbabydetails = new BabyDetails();
                newbabydetails.babyname = (string)item["babyname"];
                newbabydetails.babyid = (string)item["babyid"];
                DateTime? lastupdate = (DateTime?)item["lastupdate"];
                // TODO: convert from east europe time to local time
                newbabydetails.lastupdate = lastupdate;
                newbabydetails.displaystring = "Was last seen at: " + (string)item["lastupdate"];
                newbabydetails.baby_is_ok = true;
                // if babydetails is null, then put empty string in location, temperature and humidity 
                try
                {
                    JObject babydetails = JObject.Parse((string)item["details"]);
                    newbabydetails.location = (string)babydetails["location"];
                    newbabydetails.temperature = (float?)babydetails["temperature"];
                    newbabydetails.humidity = (float?)babydetails["humidity"];
                }
                catch 
                {
                    newbabydetails.location = "";
                    newbabydetails.temperature = null;
                    newbabydetails.humidity = null;
                }
                DisplayMessage = "Got status for " + newbabydetails.babyname;
                updatedFamilyDetails.details.Add(newbabydetails);
            }
            // update the display
            LocalFamilyDetails = updatedFamilyDetails;
        }

        private void GetFamilyDetailsThread()
        {
            while (true)
            {
                GetFamilyDetails();
                Thread.Sleep(60000);
            }
        }

        // update nearby baby details to server
        //////////////////////////////////////

        private void UpdateServer(BabyStatus sampled_baby_data, BabyDetails baby_status_from_server)
        {
            // exmple for request body
            // {
            //     "family": "family",
            //     "babyname": "ofek",
            //     "details": {
            //         "location": "<some location string>",
            //         "temprature": 0.0,
            //         "humidity": 0.0
            //     }
            // }
            // organize details and babyname and family into a json object the server expects
            JObject babydetails = new JObject
            {
                ["location"] = "<TBD LOCATION STRING>",
                ["temperature"] = sampled_baby_data._BabyTemp,
                ["humidity"] = sampled_baby_data._BabyHumd
            };
            JObject request = new JObject
            {
                ["details"] = babydetails,
                ["babyname"] = baby_status_from_server.babyname,
                ["family"] = "family"
            };

            // send http request
            HttpClient PostClient = new HttpClient();
            HttpResponseMessage resp = PostClient.PostAsync(
                updateBabyDetailsUrl,
                // serialize babyupdate
                new StringContent(
                    JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json")
                ).Result;
            var azureService = DependencyService.Get<IAzureService>();
            string response_string = azureService.UpdateBabyDetailsToServer(request).Result;

            Debug.WriteLine(response_string);
        }

        public void UpdateNearbyBabyToServer()
        {

            while (true)
            {
                var scanner = new BLEScanner();
                for (int i = 0; i < LocalFamilyDetails.details.Count; i++)
                {
                    BabyStatus result = scanner.BLEScan(LocalFamilyDetails.details[i].babyid, aes_key, dev_connect_to_ssid, dev_connect_to_pass).Result;

                    if (result != null && result._BabyTemp != null)
                    {
                        if (!scanner.dev_got_wifi_creds) // If the device has creds, he does it himself
                        {
                            Debug.WriteLine("sampled baby temp");
                            UpdateServer(result, LocalFamilyDetails.details[i]);
                        }
                        else
                        {
                            DeviceSSID = scanner.dev_wifi_ssid;
                            Console.WriteLine("Device got wifi with ssid: " + DeviceSSID);
                        }
                    }

                    Thread.Sleep(1000);
                }
            }
        }

        // handle server alert
        //////////////////////
        public void HandleAlert(string message)
        {
            // deserialize message and get alert_list
            JObject deserialize_message = JObject.Parse(message);
            JArray alert_list = (JArray)deserialize_message["alert_list"];

            // clone LocalFamilyDetails into updatedFamilyDetails
            FamilyDetails updatedFamilyDetails = new FamilyDetails();
            updatedFamilyDetails.family = LocalFamilyDetails.family;
            updatedFamilyDetails.details = new List<BabyDetails>();
            foreach (BabyDetails baby in LocalFamilyDetails.details)
            {
                updatedFamilyDetails.details.Add(baby);
            }

            // foreach alert: change list appearance and text to show alert
            foreach (JObject alert in alert_list)
            {
                // get babyname and alertreason
                string babyname = (string)alert["babyname"];
                string alertreason = (string)alert["alertreason"];
                // find baby in list

                foreach (BabyDetails baby in updatedFamilyDetails.details)
                {
                    if (baby.babyname == babyname)
                    {
                        // change baby appearance
                        baby.baby_is_ok = false;
                        baby.displaystring = alertreason;
                    }
                }
            }
            // update the display
            LocalFamilyDetails = updatedFamilyDetails;
        }


        // signalr tasks
        ////////////////
        private async Task ConnectToSignalr()
        {

            connection.On<Object>("babyalert", (data) =>
            {
                Debug.WriteLine("received new message");
                DisplayMessage = "received a new message";
                Debug.WriteLine(data.ToString());
                string receivedMessage = data.ToString();
                HandleAlert(receivedMessage);
                DisplayMessage = "handled alert";
            });

            connection.Closed += async (error) =>
            {
                Debug.WriteLine("restarting connection in 5 seconds");
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            await connection.StartAsync();

            Debug.WriteLine("connected to signalr");

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

            // // init objects for communication
            // client = new HttpClient();
            // connection = new HubConnectionBuilder().WithUrl(new Uri(baseUrl + "/api")).Build();
            // Task signalr_connection_task = Task.Run(async () => await ConnectToSignalr());
            // GetFamilyDetails();
            // Thread get_family_details_thread = new Thread(GetFamilyDetailsThread){};
            // get_family_details_thread.Start();
            // Thread update_nearby_baby_to_server_thread = new Thread(UpdateNearbyBabyToServer) { };
            // update_nearby_baby_to_server_thread.Start();
            // client.Dispose();

            StartViewModelCommand = new Command(async () => await StartViewModel());

            LocalFamilyDetails = new FamilyDetails();
            LocalFamilyDetails.details = new List<BabyDetails>();
            LocalFamilyDetails.family = "";
            DisplayMessage = "";
            DisplayMessage2 = "";
        }

        // get family details and connect to signalr on main page appear
        public Task StartViewModel()
        {
            var azureService = DependencyService.Get<IAzureService>();

            // TODO: change the following line back to: if (azureService.IsLoggedIn())
            if (true)
            {
                client = new HttpClient();
                connection = new HubConnectionBuilder().WithUrl(new Uri(baseUrl + "/api")).Build();
                Task signalr_connection_task = Task.Run(async () => await ConnectToSignalr());
                GetFamilyDetails();
                Thread get_family_details_thread = new Thread(GetFamilyDetailsThread) { };
                get_family_details_thread.Start();
                Thread update_nearby_baby_to_server_thread = new Thread(UpdateNearbyBabyToServer) { };
                update_nearby_baby_to_server_thread.Start();
                client.Dispose();

                // ViewModelNotStarted = "false";
            }

            return Task.CompletedTask;
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