using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using CounterApp.Services;
using Xamarin.Forms;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace CounterApp.Services
{
    public abstract class AzureServiceBase : IAzureService
    {
        // protected const string AzureAppName = "ilovemybabysecure";
        // protected readonly static string FunctionAppUrl = $"https://{AzureAppName}.azurewebsites.net";
        protected readonly static string FunctionAppUrl = "https://ilovemybabysecure.azurewebsites.net";
        const string AuthTokenKey = "auth-token";
        const string UserIdKey = "user-id";
        protected AzureServiceBase()
        {
            Client = new MobileServiceClient(FunctionAppUrl);
        }

        public MobileServiceClient Client { get; }

        public bool IsLoggedIn()
        {
            // TryLoadUserDetails(); 
            return Client.CurrentUser != null;
        }

        public async Task<bool> Authenticate()
        {
            // return true;
            if (IsLoggedIn())
                return true;

            try
            {
                await AuthenticateUser();
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            if (Client.CurrentUser != null)
            {
                Application.Current.Properties[AuthTokenKey] = Client.CurrentUser.MobileServiceAuthenticationToken;
                Application.Current.Properties[UserIdKey] = Client.CurrentUser.UserId;

                await Application.Current.SavePropertiesAsync();
            }

            return IsLoggedIn();
        }
        protected abstract Task AuthenticateUser();

        void TryLoadUserDetails()
        {
            if (Client.CurrentUser != null)
                return;

            if (Application.Current.Properties.TryGetValue(AuthTokenKey, out var authToken) &&
                Application.Current.Properties.TryGetValue(UserIdKey, out var userId))
            {
                Client.CurrentUser = new MobileServiceUser(userId.ToString())
                {
                    MobileServiceAuthenticationToken = authToken.ToString()
                };
            }
        }

        public async Task<string> GetFamilyDetailsFromServer()
        {
            string token = Client.CurrentUser.MobileServiceAuthenticationToken;
            HttpClient getfamilyclient = new HttpClient();
            getfamilyclient.DefaultRequestHeaders.Add("X-ZUMO-AUTH", token);
            HttpResponseMessage resp = getfamilyclient.GetAsync("https://ilovemybabysecure.azurewebsites.net/api/getfamilystatussecure").Result;
            string resp_str = resp.Content.ReadAsStringAsync().Result;

            return resp_str;
        }
        public async Task<string> UpdateBabyDetailsToServer(JObject update_request)
        {
            // send http request
            string token = Client.CurrentUser.MobileServiceAuthenticationToken;
            HttpClient PostClient = new HttpClient();
            PostClient.DefaultRequestHeaders.Add("X-ZUMO-AUTH", token);
            HttpResponseMessage resp = PostClient.PostAsync(
                "https://ilovemybabysecure.azurewebsites.net/api/updatebabystatussecure",
                // serialize babyupdate
                new StringContent(
                    JsonConvert.SerializeObject(update_request), Encoding.UTF8, "application/json")
                ).Result;

            return resp.Content.ToString();
        }

        public async Task<string> AddBabyToServer(JObject addbabyrequest)
        {
            // send http request
            string token = Client.CurrentUser.MobileServiceAuthenticationToken;
            HttpClient PostClient = new HttpClient();
            PostClient.DefaultRequestHeaders.Add("X-ZUMO-AUTH", token);
            HttpResponseMessage resp = PostClient.PostAsync(
                "https://ilovemybabysecure.azurewebsites.net/api/addbabysecure",
                // serialize babyupdate
                new StringContent(
                    JsonConvert.SerializeObject(addbabyrequest), Encoding.UTF8, "application/json")
                ).Result;

            return resp.Content.ToString();
        }
    }
}