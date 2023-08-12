using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using CounterApp.Services;
using Xamarin.Forms;

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
            TryLoadUserDetails(); 
            return Client.CurrentUser != null;
        }

        public async Task<bool> Authenticate()
        {
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
    }
}