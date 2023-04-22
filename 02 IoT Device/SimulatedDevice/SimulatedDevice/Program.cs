using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace SimulatedDevice
{
    class Program
    {
        private const int TakeSpot = 1;
        private const int LeaveSpot = 2;

        private const string DeviceConnectionString = @"HostName=SkelIotHub.azure-devices.net;DeviceId=SkeletonDevice;SharedAccessKey=jDJwlhRDQwrfUpm/0nqOk4npxnqVKlE4G9zlPu/7C3g=";

        private static readonly DeviceClient Client = DeviceClient.CreateFromConnectionString(DeviceConnectionString);

        static async Task Main(string[] args)
        {
            Console.WriteLine("Please select action number: ");
            Console.WriteLine($"({TakeSpot}) Take Spot");
            Console.WriteLine($"({LeaveSpot}) Leave Spot");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;

            while (true)
            {
                ReceiveC2dAsync();
                int action = GetSimulatedAction();
                var data = new { Action = action, ActionType = "SpotStateChange" };
                string messageJson = JsonConvert.SerializeObject(data);
                Message message = new Message(Encoding.ASCII.GetBytes(messageJson)) { ContentType = "application/json", ContentEncoding = "utf-8"};

                Console.WriteLine($"Simulating action ({action}).");
                await Client.SendEventAsync(message);
            }
        }
        private static async void ReceiveC2dAsync()
        {
            while (true)
            {
                Message receivedMessage = await Client.ReceiveAsync();
                if (receivedMessage == null) continue;

                var msg_body = receivedMessage.GetBytes();

                await Client.SendEventAsync(new Message(msg_body));
                Console.WriteLine("Received message: {0}", Encoding.ASCII.GetString(msg_body));
                await Client.CompleteAsync(receivedMessage);
            }
        }

        private static int GetSimulatedAction()
        {
            while (true)
            {
                char keyChar = Console.ReadKey(true).KeyChar;
                if (char.IsNumber(keyChar))
                {
                    int number = (int)char.GetNumericValue(keyChar);
                    if (number == TakeSpot || number == LeaveSpot)
                        return number;
                }
            }
        }

        
    }
}