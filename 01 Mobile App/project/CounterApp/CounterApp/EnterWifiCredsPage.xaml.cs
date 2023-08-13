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

            //BindingContext = new MainViewModel();

            if (((MainViewModel)(this.BindingContext)).DeviceSSID == null)
            {
                InstructionLabel.Text = "Enter Wifi name and password:";
                DevSSIDLabel.Text = "IoT Device is not connected to WiFi";
                DevSSIDLabel.TextColor = Color.Red;
            } else
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
        }
    }
}