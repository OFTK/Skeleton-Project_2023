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
        public QRCodeReader()
        {
            InitializeComponent();
        }

        private void ZXingScannerView_OnScanResult(ZXing.Result result)
        {
            BindingContext = ViewModelLocator.MainViewModel;

            try
            {
                QRData data = JsonSerializer.Deserialize<QRData>(result.Text);

                ((MainViewModel)(this.BindingContext)).AesKey = Convert.FromBase64String(data.aes_key);

                // TODO : Match baby ID and aes key

                Device.BeginInvokeOnMainThread(() => {
                    NotifyUser.Text = "QR Code located, AES-KEY acquired!";
                    NotifyUser.TextColor = Color.Green;
                });
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() => {
                    NotifyUser.Text = "Bad QR Code!";
                    NotifyUser.TextColor = Color.Red;
                });
            }
        }
    }
}