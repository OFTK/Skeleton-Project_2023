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

namespace CounterApp
{
    public class MainViewModel : INotifyPropertyChanged
    {

        // fields
        /////////

        // connection and display
        public HubConnection connection;
        private static readonly string baseUrl = "https://skeletonfunctionapp.azurewebsites.net";
        //private static readonly string baseUrl = "http://10.0.2.2:7071"; // this is the address to connect to the host
        private static readonly string getFamilyStatusUrl = baseUrl + "/api/getfamilystatus";
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
        private void UpdateBabyStatus(string babyname)
        {
            // build HTTP post request

            // send HTTP post request with nearby baby status
        }

        private void UpdateThread()
        {
            while (true)
            {
                // for each neaby baby send status to server

                Thread.Sleep(60000);
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
            // init objects for communication
            client = new HttpClient();
            connection = new HubConnectionBuilder().WithUrl(new Uri(baseUrl + "/api")).Build();
            Task signalr_connection_task = Task.Run(async () => await ConnectToSignalr());
            FamilyStatusDisplay = new FamilyStatus();
            Thread update_family_status_thread = new Thread(StatusThread){};
            update_family_status_thread.Start();
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