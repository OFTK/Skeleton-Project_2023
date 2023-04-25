using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace CounterApp
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public HubConnection connection;
        private readonly string baseUrl = "https://skeletonfunctionapp.azurewebsites.net";
        private readonly string negotiateUrl = "https://skeletonfunctionapp.azurewebsites.net/api/negotiate";
        private readonly string getCounterUrl = "https://skeletonfunctionapp.azurewebsites.net/api/getcounter";
        private readonly string updateCounterUrl = "https://skeletonfunctionapp.azurewebsites.net/api/updatecounter";
        public HttpClient client;

        private int _counter1;
        class SimpleCounter { public int count = 0; };
        class SignalRConnection
        {
            public string url = "";
            public string AccessToken = "";
        };

        private string _displayMessage;
        public string DisplayMessage {get => _displayMessage; set => SetProperty(ref _displayMessage, value); }
        public int Counter1
        {
            get => _counter1;
            set => SetProperty(ref _counter1, value);
        }

        private int _counter2;
        public int Counter2
        {
            get => _counter1;
            set => SetProperty(ref _counter2, value);
        }

        public ICommand IncrementCounterCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            Counter1 = 0;
            Counter2 = 0;
            client = new HttpClient();
            IncrementCounterCommand = new Command<string>(IncrementCounterById);
            Counter1 = GetOnlineCounter();
//            HttpResponseMessage resp = client.GetAsync(new Uri(negotiateUrl)).Result;
//            string negotiateresponse = "";
//            if (resp.IsSuccessStatusCode)
//            {
//                negotiateresponse = resp.Content.ReadAsStringAsync().Result;
//            }
            connection = new HubConnectionBuilder().WithUrl(new Uri(baseUrl + "/api")).Build();
            Task task = Task.Run(async () => await ConnectToSignalr());
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task ConnectToSignalr()
        {
            connection.On<string>("counterUpdate", (data) =>
            {
                Counter1 = GetOnlineCounter();
            });
            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
            await connection.StartAsync();
        }

        private async Task InvokeSignalr(int updatedcounter)
        {
            await connection.InvokeAsync("counterUpdate", updatedcounter);
            //await Clients.All.SendAsync("counterUpdate", updatedcounter);
        }


        private int GetOnlineCounter()
        {
            HttpResponseMessage resp = client.GetAsync(new Uri(getCounterUrl)).Result;
            if (resp.IsSuccessStatusCode)
            {
                string webcounterstring = resp.Content.ReadAsStringAsync().Result;
                DisplayMessage = webcounterstring;
                SimpleCounter storagejsoncounter = JsonConvert.DeserializeObject<SimpleCounter>(webcounterstring);
                int storagecounter = storagejsoncounter.count;
                return storagecounter;
            }
            else { return 0; }
        }

        private void IncrementCounterById(string counterId)
        {

            int oldcounter = GetOnlineCounter();
            int newcounter = oldcounter + 1;
            _ = client.GetAsync(string.Format("{0}?counter={1}", updateCounterUrl, newcounter)).Result;
            Counter1 = newcounter;
            Task task = Task.Run(async () => await InvokeSignalr(newcounter));
        }

        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }


        private void PerformInitialConnection()
        {
        }
    }
}