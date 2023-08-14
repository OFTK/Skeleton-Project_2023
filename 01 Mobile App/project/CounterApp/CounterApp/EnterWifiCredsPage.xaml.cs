using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CounterApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EnterWifiCredsPage : ContentPage
    {
        public EnterWifiCredsPage()
        {
            InitializeComponent();

            Set_Labels();
        }

        private void Set_Labels()
        {
            BindingContext = ViewModelLocator.MainViewModel;

            if (((MainViewModel)(this.BindingContext)).DeviceSSID == null)
            {
                InstructionLabel.Text = "Enter Wifi name and password:";

                if (((MainViewModel)(this.BindingContext)).DevConnToSSID == null)
                {
                    DevSSIDLabel.Text = "IoT Device is not connected to WiFi";
                    DevSSIDLabel.TextColor = Color.Red;
                }
                else if (((MainViewModel)(this.BindingContext)).AesKey == null)
                {
                    DevSSIDLabel.Text = "No encryption key, scan the device's QR code!";
                    DevSSIDLabel.TextColor = Color.Red;
                } 
                else
                {
                    DevSSIDLabel.Text = $"IoT Device is trying to connect to {((MainViewModel)(this.BindingContext)).DevConnToSSID}";
                    DevSSIDLabel.TextColor = Color.OrangeRed;
                }
            }
            else
            {
                InstructionLabel.Text = "Enter another Wifi name and password (The device is already connected):";
                DevSSIDLabel.Text = $"IoT Device is connected to {((MainViewModel)(this.BindingContext)).DeviceSSID.Trim()}";
                DevSSIDLabel.TextColor = Color.Green;
            }
        }

        private void Submit_Clicked(object sender, EventArgs e)
        {
            ((MainViewModel)(this.BindingContext)).DevConnToSSID = SSIDEntry.Text;
            ((MainViewModel)(this.BindingContext)).DevConnToPass = PassEntry.Text;

            Set_Labels();
        }
    }
}