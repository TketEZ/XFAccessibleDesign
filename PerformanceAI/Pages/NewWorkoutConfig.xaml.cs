using System;
using System.Collections.Generic;
using PerformanceAI.Helpers;
using PerformanceAI.Views;
using Xamarin.Forms;

namespace PerformanceAI.Pages
{
    public partial class NewWorkoutConfig : ContentView
    {
        // todo: add validation to Next button that checks if items have been selected

        public NewWorkoutConfig()
        {
            InitializeComponent();
            BindingContext = this;

            TrainingIntentPicker.SelectedItem = Settings.TrainingIntent.ToString();
            ActivityTypePicker.SelectedItem = Settings.ActivityType.ToString();
        }

        async void NewWorkoutNextButton_Clicked(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new NewWorkouts());
        }

        void TrainingIntentPicker_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            string _trainingIntent = TrainingIntentPicker.SelectedItem.ToString();

            Settings.TrainingIntent = _trainingIntent;
        }

        void ActivityTypePicker_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            string _activityType = ActivityTypePicker.SelectedItem.ToString();

            Settings.ActivityType = _activityType;
        }
    }
}
