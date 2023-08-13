using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Linq;
using System.Text;
using Xamarin.Forms.Xaml;
using System;

namespace CounterApp
{
    public class LoginViewModel : BaseViewModel
    {
        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await Login());
        }

        public ICommand LoginCommand { get; }

        private async Task Login()
        {
            var authResult = await WebAuthenticator.AuthenticateAsync(
                    new Uri("https://ilovemybabysecure.azurewebsites.net/.auth/login/aad"),
                    new Uri("myapp://"));

            string accessToken = authResult?.AccessToken;
        }
    }
}