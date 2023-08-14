using CounterApp.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static CounterApp.MainViewModel;

namespace CounterApp
{
    public class QRData
    {
        public string baby_id { get; set; }
        public string aes_key { get; set; }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QRCodeReader : ContentPage
    {
        private QRData scanner_data;

        public QRCodeReader()
        {
            InitializeComponent();
        }

        private void ZXingScannerView_OnScanResult(ZXing.Result result)
        {
            bool found_baby = false;
            BindingContext = ViewModelLocator.MainViewModel;

            try
            {
                scanner_data = JsonSerializer.Deserialize<QRData>(result.Text);

                ((MainViewModel)(this.BindingContext)).AesKey = Convert.FromBase64String(scanner_data.aes_key);
                string baby_id = scanner_data.baby_id;

                for (int i = 0;  i < ((MainViewModel)(this.BindingContext)).LocalFamilyDetails.details.Count;  i++)
                {
                    if (((MainViewModel)(this.BindingContext)).LocalFamilyDetails.details[i].babyid == baby_id)
                    {
                        found_baby = true;
                    }
                }

                if (!found_baby)
                {
                    Device.BeginInvokeOnMainThread(() => {
                        NotifyUser.Text = "This is a new baby! insert his\\hers name:";
                        NewBabyName.Placeholder = "New baby name";
                        NewBabyName.IsVisible = true;
                        NewBabyButton.Text = "Submit";
                        NewBabyButton.IsVisible = true;
                    });

                } else
                {
                    Device.BeginInvokeOnMainThread(() => {
                        NotifyUser.Text = "QR Code located, AES-KEY acquired!";
                        NotifyUser.TextColor = Color.Green;
                    });
                }
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => {
                    NotifyUser.Text = "Bad QR Code!";
                    NotifyUser.TextColor = Color.Red;
                });
            }
        }

        private void NewBabyButton_Clicked(object sender, EventArgs e)
        {
            JObject request = new JObject
            {
                ["family"] = "family",
                ["babyname"] = NewBabyName.Text,
                ["babyid"] = scanner_data.baby_id
            };

            var azureService = DependencyService.Get<IAzureService>();
            string response_string = azureService.AddBabyToServer(request).Result;
        }
    }
}