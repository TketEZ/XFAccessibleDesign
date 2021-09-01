using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PerformanceAI.Models;
using PerformanceAI.Services;
using Xamarin.Forms;

namespace PerformanceAI.Views
{
    public partial class TrainingZoneDetailPage : ContentPage
    {
        List<string> DurationPickerList = new List<string>()
        {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"
        };

        List<string> ZonePickerList = new List<string>()
        {
            "1",
            "2",
            "3",
            "4",
            "5"
        };

        private TrainingZoneModel _trainingZoneItem;
        public TrainingZoneModel TrainingZoneItem
        {
            get { return _trainingZoneItem; }
            set { _trainingZoneItem = value; }
        }

        public TrainingZoneDetailPage(TrainingZoneModel TrainingZone)
        {
            InitializeComponent();
            BindingContext = this;
            _trainingZoneItem = TrainingZone;
            ZonePicker.ItemsSource = ZonePickerList;
            DurationPicker.ItemsSource = DurationPickerList;

            // hide delete button for first item 
            if (TrainingZoneItem != null &&
                TrainingZoneItem.Id == 0 &&
                TrainingZoneItem.Zone == null &&
                TrainingZoneItem.durationInMinutes == 0)
            {
                DeleteBtn.IsVisible = false;
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync(TrainingZoneItem);
        }

        private async Task LoadDataAsync(TrainingZoneModel trainingZone)
        {
            WorkoutDatabase database = await WorkoutDatabase.Instance;
            var theTrainingZone = await database.GetTrainingZoneAsync(trainingZone.Id);

            if (theTrainingZone == null)
            {
                return;
            }

            Console.WriteLine($"Training zone id: {theTrainingZone.Id}");
            DurationPicker.SelectedItem = theTrainingZone.durationInMinutes.ToString();
            ZonePicker.SelectedItem = theTrainingZone.Zone;
        }

        int TrainingZoneId = 0;
        async void OnZoneSaved(System.Object sender, System.EventArgs e)
        {
            if (DurationPicker.SelectedIndex != -1 && ZonePicker.SelectedIndex != -1)
            {
                double ZoneDuration = double.Parse(DurationPicker.SelectedItem.ToString());
                TimeSpan ZoneDurationTs = TimeSpan.FromMinutes(ZoneDuration);

                TrainingZoneItem.Zone = ZonePicker.SelectedItem.ToString();
                TrainingZoneItem.durationInMinutes = ZoneDurationTs.TotalMinutes;

                // establish database connection
                WorkoutDatabase database = await WorkoutDatabase.Instance;

                // save item
                var result = await database.AddTrainingZoneAsync(TrainingZoneItem);
                Console.WriteLine($"Id of newly added zone: {TrainingZoneItem.Id}");
                Console.WriteLine($"number of new records added: {result}");

                TrainingZoneId = TrainingZoneItem.Id;

                // handle navigation
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Missing Data", "Please make sure to select a training zone and duration before trying to save.", "Ok");
            }
        }

        async void DeleteZone_Clicked(System.Object sender, System.EventArgs e)
        {
            if (TrainingZoneItem == null)
            {
                return;
            }

            if (!await DisplayAlert("Delete training zone?", "This will permanently delete the zone data. Are you sure you want to delete this training zone?", "Yes", "Cancel"))
            {
                return;
            }

            Console.WriteLine($"Deleting training zone with id: {TrainingZoneItem.Id}");

            try
            {
                WorkoutDatabase database = await WorkoutDatabase.Instance;
                await database.DeleteTrainingZoneAsync(TrainingZoneItem.Id);
                Console.WriteLine($"Finished deleting trainingZone with id: {TrainingZoneItem.Id}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Deletion error", "Unable to delete training zone. Please try again.", "Ok");
                Console.WriteLine($"Removal error: {ex.Message}");
            }

            await Navigation.PopAsync();
        }

    }
}
