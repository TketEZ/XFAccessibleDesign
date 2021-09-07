

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerformanceAI.Views;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Forms;

namespace PerformanceAI
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async void ButtonNewWorkout_Clicked(System.Object sender, System.EventArgs e)
        {
            //todo: if device is connected, navigate to new workout directly, otherwise go to scanning page
            await Navigation.PushAsync(new NewWorkouts());
        }

        async void ButtonOldWorkout_Clicked(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new OldWorkouts());
        }

        void TabViewItem_TabTapped(System.Object sender, Xamarin.CommunityToolkit.UI.Views.TabTappedEventArgs e)
        {
        }
    }
}
