using System.Threading.Tasks;
using CounterApp.Services;
using Microsoft.WindowsAzure.MobileServices;
using Plugin.CurrentActivity;

[assembly: Xamarin.Forms.Dependency(typeof(CounterApp.Droid.Services.AzureService))]
namespace CounterApp.Droid.Services
{
    public class AzureService : AzureServiceBase
    {
        protected override Task AuthenticateUser()
        {
            return Client.LoginAsync(CrossCurrentActivity.Current.Activity,
                                    MobileServiceAuthenticationProvider.WindowsAzureActiveDirectory,
                                    "counterapp");
        }
    }
}