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
using System.Security.Cryptography;
using System.IO;

namespace CounterApp
{
    class BLEScanner
    {
        private readonly string TEMP_CHAR_UUID = "f96c20eb-05c7-4c31-803b-03428eae9aa2";
        private readonly string HUMD_CHAR_UUID = "2d4fa781-cf1c-4ea1-9427-14951f794d80";
        private readonly string SYNC_CHAR_UUID = "3b4fa77b-bb0b-4b12-8ee6-913382a4f2a0";
        private readonly string WIFI_SSID_CHAR_UUID = "28919cc6-36d5-11ee-be56-0242ac120002";
        private readonly string WIFI_PASS_CHAR_UUID = "02350feb-8302-4ff7-8f04-9e07f69d73df";

        // Baby Status

        private readonly IAdapter _bluetoothAdapter;                            // Class for the Bluetooth adapter
        private readonly List<IDevice> _gattDevices = new List<IDevice>();      // Empty list to store BLE devices that can be detected by the Bluetooth adapter

        private Aes aes = AesManaged.Create();
        public bool dev_got_wifi_creds;
        private bool dev_change_wifi_pass;
        public string dev_wifi_ssid;

        private byte[] EncryptString(string plainText)
        {
            byte[] array;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }
                    array = memoryStream.ToArray();
                }
            }

            return array;
        }

        // @note: Pads with zeroes to dwords
        private byte[] string_to_byte_arr(string str, bool encrypt = false)
        {
            if (encrypt)
            {
                return EncryptString(str);
            }
            else
            {
                byte[] arr = new byte[str.Length];

                int i;

                for (i = 0; i < str.Length; i++)
                {
                    arr[i] = Convert.ToByte(str[i]);
                }

                for (; i < arr.Length; i++)
                {
                    arr[i] = 0;
                }

                return arr;
            }
        }

        public async Task<MainViewModel.BabyStatus> BLEScan(string ble_uuid,
                                                            string wifi_ssid = null,
                                                            string wifi_pass = null)
        {
            // TODO : This is debug! Do the real thing!
            wifi_ssid = "Home";
            wifi_pass = "097452430";
            // ----------------------------------------
            // TODO : Get the key from cloud
            byte[] aes_key = new byte[16] { 0xa1, 0x95, 0x1f, 0x50, 0xe5, 0x66, 0x8b, 0xb7, 0x23, 0xd4, 0xfa, 0x8a, 0xb3, 0x5a, 0xef, 0x14 }; // Constant for every iot dev
            byte[] aes_sync = new byte[16];
            bool got_sync = false;
            // ----------------------------------------

            try 
            {
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
                                else if (wifi_ssid != null && character.Uuid.ToString() == SYNC_CHAR_UUID)
                                {
                                    if (character.CanRead)
                                    {
                                        aes_sync = await character.ReadAsync();

                                        if (aes_sync.Length * 8 != aes.BlockSize)
                                        {
                                            Console.WriteLine("Received bad sync of size " + aes_sync.Length.ToString() + ", need to be: " + aes.BlockSize / 8);
                                            for (int i = 0; i < aes_sync.Length; i++)
                                            {
                                                if (i > 0) Console.Write((":"));
                                                Console.Write("{0:X}", aes_sync[i]);
                                            }
                                            Console.WriteLine("");
                                        }
                                        else
                                        {
                                            // Set up the algorithm
                                            aes.Padding = PaddingMode.PKCS7;
                                            aes.Mode = CipherMode.CBC;
                                            aes.Key = aes_key;
                                            aes.IV = aes_sync;
                                            got_sync = true;
                                        }
                                    }
                                }
                                else if (character.Uuid.ToString() == WIFI_SSID_CHAR_UUID) // We also check here if dev got wifi creds
                                {
                                    if (character.CanRead && character.CanWrite && got_sync)
                                    {
                                        // First, we check if the IOT device is connected to the network we want it to be connected to
                                        byte[] receivedBytes = await character.ReadAsync();

                                        if (receivedBytes.Length > 0 && receivedBytes[0] != 0)
                                        {
                                            dev_wifi_ssid = Encoding.ASCII.GetString(receivedBytes);
                                            Console.WriteLine("IOT Dev got ssid, using \"" + dev_wifi_ssid + "\"");
                                            dev_got_wifi_creds = true;

                                            if (wifi_ssid != null && !wifi_ssid.Equals(dev_wifi_ssid)) // We want to change ssid
                                            {
                                                dev_change_wifi_pass = true;
                                                byte[] b_wifi_ssid = string_to_byte_arr(wifi_ssid);
                                                await character.WriteAsync(b_wifi_ssid);
                                            } else
                                            {
                                                dev_change_wifi_pass = false;
                                            }

                                        } else
                                        {
                                            Console.WriteLine("IOT Dev got no ssid!");
                                            dev_got_wifi_creds = false;
                                            
                                            if (wifi_ssid != null)
                                            {
                                                byte[] b_wifi_ssid = string_to_byte_arr(wifi_ssid);
                                                await character.WriteAsync(b_wifi_ssid);
                                            }
                                        }
                                    }
                                }
                                else if (dev_change_wifi_pass && wifi_pass != null && character.Uuid.ToString() == WIFI_PASS_CHAR_UUID)
                                {
                                    if (character.CanWrite && got_sync)
                                    {
                                        byte[] b_wifi_pass = string_to_byte_arr(wifi_pass, true); // Encrypting password
                                        await character.WriteAsync(b_wifi_pass);
                                    }
                                }
                            }
                        }
                    }
                }
                return status;
            } catch {

            }
            
            // In case something went wrong
            return null;
        }

        public BLEScanner()
        {
            _bluetoothAdapter = CrossBluetoothLE.Current.Adapter;               // Point _bluetoothAdapter to the current adapter on the phone
            _bluetoothAdapter.DeviceDiscovered += (sender, foundBleDevice) =>   // When a BLE Device is found, run the small function below to add it to our list
            {
                if (foundBleDevice.Device != null && !string.IsNullOrEmpty(foundBleDevice.Device.Name))
                    _gattDevices.Add(foundBleDevice.Device);
            };

            dev_got_wifi_creds = false;
            dev_change_wifi_pass = true;
            dev_wifi_ssid = "";
        }
    }
}
