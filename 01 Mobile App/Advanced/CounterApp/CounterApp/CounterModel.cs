using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;


namespace CounterApp
{
    public class connectionInfo
    {
        public string connectionUrl { get; set; }
        public string accessToken { get; set; }
    }

    public class CounterModel : INotifyPropertyChanged
    {
        public int Id { get; }

        private int _counter;
        public int Counter
        {
            get => _counter;
            set => SetProperty(ref _counter, value);
        }

        public HubConnection connection;
        public connectionInfo connectionDetails;
        private static HttpClient http = new HttpClient();
        //private readonly String baseUrl = "https://skeletonfunctionapp.azurewebsites.net";
        private readonly string negotiateUrl =      "https://skeletonfunctionapp.azurewebsites.net/api/negotiate";
        private readonly string getCounterUrl =     "https://skeletonfunctionapp.azurewebsites.net/api/getcounter";
        private readonly string updateCounterUrl =  "https://skeletonfunctionapp.azurewebsites.net/api/updatecounter";
        
        // private readonly string negotiateUrl = "http://localhost:7071/api/negotiate";
        // private readonly string getCounterUrl = "http://localhost:7071/api/getcounter";
        // private readonly string updateCounterUrl = "http://localhost:7071/api/updatecounter";

        public ICommand IncrementCounterCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public CounterModel(int id)
        {
            Id = id;
            IncrementCounterCommand = new Command(() =>
                Counter = JsonDocument.Parse(
                    http.GetAsync(string.Format("{0}?counter={1}", updateCounterUrl, (Counter+1)))
                    .ToString()
                ).RootElement.GetProperty("current-counter").GetInt32()
            );

            // TODO: might need to add accessToken
            connection = new HubConnectionBuilder().WithUrl(
                JsonDocument.Parse(
                    http.PostAsync(
                        negotiateUrl, 
                        new StringContent("{ UserId: SomeUser }", Encoding.UTF8, "application/json")
                    ).ToString()
                ).RootElement.GetProperty("url").GetString()
            ).Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            connection.On<string>("CounterUpdate", data => {
                Counter = JsonDocument.Parse(
                    http.GetAsync(getCounterUrl).ToString())
                .RootElement.GetProperty("count").GetInt32();
            });

        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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