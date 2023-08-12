﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.CommunityToolkit.Extensions;
using CounterApp.Services;
using static CounterApp.MainViewModel;

namespace CounterApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = ViewModelLocator.MainViewModel;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            // get premissions
            if (!await PermissionsGrantedAsync()) // Make sure there is permission to use Bluetooth
            {
                await Application.Current.MainPage.DisplayAlert("Permission required", "Application needs location permission", "OK");
                return;
            }

            var azureService = DependencyService.Get<IAzureService>();

            if (!azureService.IsLoggedIn())
            {
                if (!Navigation.ModalStack.Any())
                    await Navigation.PushModalAsync(new LoginPage(), false);
            }

            ((MainViewModel)this.BindingContext).OnAppearing();
        }

        private async Task<bool> PermissionsGrantedAsync()      // Function to make sure that all the appropriate approvals are in place
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

        void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null) return;
            MainViewModel.BabyDetails tappedBaby = (MainViewModel.BabyDetails)e.Item;

            // create popup with baby's details
            var popup = new Popup
            {
                Content = new StackLayout{Children =
                {
                    new Label {Style=Device.Styles.TitleStyle, Text = tappedBaby.babyname},
                    new Label {Style=Device.Styles.BodyStyle, Text = "babytag ID: " + tappedBaby.babyid },
                    new Label {Style=Device.Styles.BodyStyle, Text = "last update: " + tappedBaby.lastupdate},
                    new Label {Style=Device.Styles.BodyStyle, Text = "temprature: " + tappedBaby.temperature },
                    new Label {Style=Device.Styles.BodyStyle, Text = "humidity: " + tappedBaby.humidity},
                    new Label {Style=Device.Styles.BodyStyle, Text = "location: " + tappedBaby.location},
                }}
            };
            App.Current.MainPage.Navigation.ShowPopup(popup);

            ((ListView)sender).SelectedItem = null;
        }

        private void FamilyStatusListView_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged();


            if (this.BindingContext != null)
            {
                MainViewModel.FamilyDetails familyDetails = ((MainViewModel)this.BindingContext).LocalFamilyDetails;
                bool baby_alert = true;
                if (familyDetails != null)
                {
                    foreach (MainViewModel.BabyDetails baby in familyDetails.details)
                    {
                        if (baby.baby_is_ok == false)
                        {
                            baby.baby_is_ok = true;
                            baby_alert = false;
                        }
                    }

                    if (baby_alert == false)
                    {
                        Application.Current.MainPage.DisplayAlert("Baby Alert", "check out baby details", "OK");
                    }

                }
            }
        }
    }
}
