using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PerformanceAI.Helpers;
using Xamarin.Forms;

namespace PerformanceAI.Views
{
    public partial class HrZones : ContentPage
    {

        public HrZones()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            MaxHr.Text = Settings.MaxHr.ToString();

            HrZone1LowerBound.Text = Settings.HrZone1LowerBound.ToString();
            HrZone1UpperBound.Text = Settings.HrZone1UpperBound.ToString();
            HrZone2UpperBound.Text = Settings.HrZone2UpperBound.ToString();
            HrZone3UpperBound.Text = Settings.HrZone3UpperBound.ToString();
            HrZone4UpperBound.Text = Settings.HrZone4UpperBound.ToString();
            HrZone5UpperBound.Text = Settings.MaxHr.ToString();

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            Settings.MaxHr = Convert.ToInt32(MaxHr.Text);

            Settings.HrZone1LowerBound = Convert.ToInt32(HrZone1LowerBound.Text);
            Settings.HrZone1UpperBound = Convert.ToInt32(HrZone1UpperBound.Text);
            Settings.HrZone2UpperBound = Convert.ToInt32(HrZone2UpperBound.Text);
            Settings.HrZone3UpperBound = Convert.ToInt32(HrZone3UpperBound.Text);
            Settings.HrZone4UpperBound = Convert.ToInt32(HrZone4UpperBound.Text);
        }

    }
}
