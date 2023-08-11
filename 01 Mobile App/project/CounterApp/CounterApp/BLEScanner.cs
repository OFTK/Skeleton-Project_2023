using System;
using System.Collections.Generic;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using XamarinEssentials = Xamarin.Essentials;
using Xamarin.Forms.Xaml;


namespace CounterApp
{
    class BLEScanner
    {
        private readonly string TEMP_CHAR_UUID = "f96c20eb-05c7-4c31-803b-03428eae9aa2";
        private readonly string HUMD_CHAR_UUID = "2d4fa781-cf1c-4ea1-9427-14951f794d80";
        private readonly string WIFI_CHAR_UUID = "28919cc6-36d5-11ee-be56-0242ac120002";

        // Baby Status

        private readonly IAdapter _bluetoothAdapter;                            // Class for the Bluetooth adapter
        private readonly List<IDevice> _gattDevices = new List<IDevice>();      // Empty list to store BLE devices that can be detected by the Bluetooth adapter

        // Wifi ops enum
        enum WIFI_OPS
        {
            WIFI_OPS_SSID = 0,
            WIFI_OPS_PASS = 1,
            WIFI_OPS_URL = 2
        };

        private static void string_to_byte_arr(string str, ref byte[] byte_arr, int offset)
        {
            for (int i = offset; i < str.Length + 1; i++)
            {
                byte_arr[i] = Convert.ToByte(str[i - offset]);
            }
        }

        public async Task<MainViewModel.BabyStatus> BLEScan(string ble_uuid,
                                                            string wifi_ssid = null,
                                                            string wifi_pass = null,
                                                            string wifi_url = null)
        {

            // TODO : This is debug! Do the real thing!
            wifi_ssid = "Home";
            wifi_pass = "097452430";
            wifi_url = "abcdefg";
            // ----------------------------------------

            // initialize bluetooth adapter
            if (!_bluetoothAdapter.IsScanning)                                                             // Make sure that the Bluetooth adapter is scanning for devices
            {
                await _bluetoothAdapter.StartScanningForDevicesAsync();
            }

            // initialize status object (that we would return eventually)
            var status = new MainViewModel.BabyStatus();

            // scan all devices around
            foreach (IDevice device in _bluetoothAdapter.ConnectedDevices)                                // Make sure BLE devices are added to the _gattDevices list
                _gattDevices.Add(device);

            foreach (IDevice device in _gattDevices)                                                      // Make sure BLE devices are added to the _gattDevices list
            {

                // connect to device that has a name TINOKEBLE
                if (device.Name.Equals("TINOKIBLE"))
                {
                    // connect to device if disconnected
                    if (device.State != DeviceState.Connected)                                            // Check first if we are already connected to the BLE Device 
                    {
                        try
                        {
                            var connectParameters = new ConnectParameters(false, true);
                            await _bluetoothAdapter.ConnectToDeviceAsync(device, connectParameters);          // if we are not connected, then try to connect to the BLE Device selected
                        }
                        catch
                        {
                            await Application.Current.MainPage.DisplayAlert("Error connecting", $"Error connecting to BLE device: {device.Name ?? "N/A"}", "Retry");       // give an error message if it is not possible to connect
                            continue;
                        }
                    }

                    // check all services
                    var servicesListReadOnly = await device.GetServicesAsync();           // Read in the Services available
                    IService TINOKI_Service = null;

                    foreach (IService service in servicesListReadOnly)
                    {
                        if (service.Id.ToString() == ble_uuid)
                        {
                            TINOKI_Service = service;
                            break;
                        }
                    }

                    // read services only if the device has the required uuid
                    if (TINOKI_Service == null)
                        await _bluetoothAdapter.DisconnectDeviceAsync(device);
                    else
                    {
                        var charListReadOnly = await TINOKI_Service.GetCharacteristicsAsync();       // Read in available Characteristics

                        foreach (ICharacteristic character in charListReadOnly) // Reading humidity and temperature
                        {
                            if (character.Uuid.ToString() == TEMP_CHAR_UUID)
                            {
                                if (character.CanRead)
                                {
                                    byte[] receivedBytes = await character.ReadAsync();

                                    if (receivedBytes != null)
                                        status._BabyTemp = BitConverter.ToSingle(receivedBytes, 0);
                                }

                                status._BabyLastSeenTime = DateTime.Now;
                            }
                            else if (character.Uuid.ToString() == HUMD_CHAR_UUID)
                            {
                                if (character.CanRead)
                                {
                                    byte[] receivedBytes = await character.ReadAsync();

                                    if (receivedBytes != null)
                                        status._BabyHumd = BitConverter.ToSingle(receivedBytes, 0);
                                }
                            }
                            else if (wifi_ssid != null && character.Uuid.ToString() == WIFI_CHAR_UUID)
                            {
                                if (character.CanRead && character.CanWrite)
                                {
                                    // First, we check if the IOT device is connected to the network we want it to be connected
                                    byte[] receivedBytes = await character.ReadAsync();

                                    if (receivedBytes == null || !wifi_ssid.Equals(Encoding.ASCII.GetString(receivedBytes)))
                                    { // If it is not, sending the wifi creds
                                      // Converting strings to byte arrays

                                        Console.WriteLine(Encoding.ASCII.GetString(receivedBytes));
                                        Console.WriteLine(wifi_ssid);

                                        byte[] b_wifi_ssid = new byte[wifi_ssid.Length + 1];
                                        b_wifi_ssid[0] = (byte)WIFI_OPS.WIFI_OPS_SSID;
                                        string_to_byte_arr(wifi_ssid, ref b_wifi_ssid, 1);

                                        byte[] b_wifi_pass = new byte[wifi_pass.Length + 1];
                                        b_wifi_pass[0] = (byte)WIFI_OPS.WIFI_OPS_PASS;
                                        string_to_byte_arr(wifi_pass, ref b_wifi_pass, 1);

                                        byte[] b_wifi_url = new byte[wifi_url.Length + 1];
                                        b_wifi_url[0] = (byte)WIFI_OPS.WIFI_OPS_URL;
                                        string_to_byte_arr(wifi_url, ref b_wifi_url, 1);

                                        await character.WriteAsync(b_wifi_ssid);
                                        await character.WriteAsync(b_wifi_pass);
                                        await character.WriteAsync(b_wifi_url);
                                    }
                                }
                            }
                        }
                    }
                }

            }

            return status;

        }

        public BLEScanner()
        {
            _bluetoothAdapter = CrossBluetoothLE.Current.Adapter;               // Point _bluetoothAdapter to the current adapter on the phone
            _bluetoothAdapter.DeviceDiscovered += (sender, foundBleDevice) =>   // When a BLE Device is found, run the small function below to add it to our list
            {
                if (foundBleDevice.Device != null && !string.IsNullOrEmpty(foundBleDevice.Device.Name))
                    _gattDevices.Add(foundBleDevice.Device);
            };
        }


    }
}
