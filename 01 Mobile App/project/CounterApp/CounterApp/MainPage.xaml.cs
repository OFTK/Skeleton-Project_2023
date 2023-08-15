using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using CounterApp.Services;
using Xamarin.CommunityToolkit.Extensions;

namespace CounterApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = ViewModelLocator.MainViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Permissions check

            var azureService = DependencyService.Get<IAzureService>();

            if (!azureService.IsLoggedIn())
            {
                if (!Navigation.ModalStack.Any())
                    await Navigation.PushModalAsync(new LoginPage(), false);
            }
        }

        private async Task<bool> CheckLocationPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            return status == PermissionStatus.Granted;
        }

        private void WifiBut_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new EnterWifiCredsPage());
        }

        private void QRBut_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new QRCodeReader());
        }

        private async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            var tappedBaby = (MainViewModel.BabyDetails)e.SelectedItem;

            var popupContent = new StackLayout
            {
                BackgroundColor = Color.White,
                Padding = new Thickness(20),
                Children =
                {
                    new Label { Style=Device.Styles.TitleStyle, Text = tappedBaby.babyname },
                    new Label { Style=Device.Styles.BodyStyle, Text = $"babytag ID: {tappedBaby.babyid}" },
                    new Label { Style=Device.Styles.BodyStyle, Text = $"last update: {tappedBaby.lastupdate}" },
                    new Label { Style=Device.Styles.BodyStyle, Text = $"temprature: {tappedBaby.temperature}" },
                    new Label { Style=Device.Styles.BodyStyle, Text = $"humidity: {tappedBaby.humidity}" },
                    new Label { Style=Device.Styles.BodyStyle, Text = $"location: {tappedBaby.location}" },
                }
            };

            var popup = new Xamarin.CommunityToolkit.UI.Views.Popup
            {
                Content = popupContent
            };

            App.Current.MainPage.Navigation.ShowPopup(popup);

            FamilyStatusListView.SelectedItem = null;
        }
    }
}
