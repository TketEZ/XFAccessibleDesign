using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PerformanceAI.Helpers;
using PerformanceAI.Models;
using PerformanceAI.Services;
using PerformanceAI.Utils;
using Xamarin.Forms;
using static PerformanceAI.Utils.HrZoneGetter;

namespace PerformanceAI.Views
{
    public partial class OldWorkoutDetailPage : ContentPage
    {

        private WorkoutModel _workoutItem;
        public WorkoutModel WorkoutItem
        {
            get
            {
                return _workoutItem;
            }
            set
            {
                _workoutItem = value;
            }
        }

        public OldWorkoutDetailPage(WorkoutModel workout)
        {
            InitializeComponent();
            BindingContext = this;
            _workoutItem = workout;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDataAsync(WorkoutItem);
        }

        private async Task LoadDataAsync(WorkoutModel WorkoutItem)
        {
            WorkoutDatabase database = await WorkoutDatabase.Instance;
            var theWorkout  = await database.GetWorkoutAsync(WorkoutItem.Id);

            if (theWorkout != null)
            {
                // get heart rate data
                string HeartRateDataJson = theWorkout.HeartRateValues;
                List<int> HeartRateDataList = JsonConvert.DeserializeObject<List<int>>(HeartRateDataJson);

                // get count of heart rate data points
                int HeartRateDataCount = HeartRateDataList.Count();

                // get % of time spent in each Heart rate zone 
                var HrZonePercentages = GetTimeSpentInEachHrZone(HeartRateDataCount, HeartRateDataList);

                // get workout duration as nicely formatted string
                string workoutDuration = GetWorkoutDuration(theWorkout.WorkoutDurationInTicks);

                // Update UI
                UpdateUI(theWorkout, HrZonePercentages, workoutDuration);
            }
        }

        private string GetWorkoutDuration(long workoutDurationInTicks)
        {
            TimeSpan workoutDuration = TimeSpan.FromTicks(workoutDurationInTicks);
            string workoutDurationString = string.Format("{0:00}:{1:00}:{2:00}", workoutDuration.Hours, workoutDuration.Minutes, workoutDuration.Seconds);
            return workoutDurationString;
        }

        private void UpdateUI(WorkoutModel theWorkout, (double Zone1Percentage, double Zone2Percentage, double Zone3Percentage, double Zone4Percentage, double Zone5Percentage) hrZonePercentages, string workoutDuration)
        {
            FinishDateLbl.Text = $"Workout completed on: {theWorkout.LongDate} ";
            FinishTimeLbl.Text = $"Workout finished at: {theWorkout.LongTime} ";
            SessionMaxHrLbl.Text = $"Peak heart rate: {theWorkout.SessionMaxHr.ToString()} ";
            SessionAvgHrLbl.Text = $"Average heart rate: {theWorkout.SessionAvgHr.ToString()} ";
            SessionMinHrLbl.Text = $"Lowest heart rate: {theWorkout.SessionAvgHr.ToString()} ";
            sessionDurationLbl.Text = $"Workout duration: {workoutDuration} ";

            TimeSpentInZone1Lbl.Text = $"Zone 1: {hrZonePercentages.Zone1Percentage}% ";
            TimeSpentInZone2Lbl.Text = $"Zone 2: {hrZonePercentages.Zone2Percentage}% ";
            TimeSpentInZone3Lbl.Text = $"Zone 3: {hrZonePercentages.Zone3Percentage}% ";
            TimeSpentInZone4Lbl.Text = $"Zone 4: {hrZonePercentages.Zone4Percentage}% ";
            TimeSpentInZone5Lbl.Text = $"Zone 5: {hrZonePercentages.Zone5Percentage}% ";
        }

        private (double PercentageOfSessionSpentInZone1,
            double PercentageOfSessionSpentInZone2,
            double PercentageOfSessionSpentInZone3,
            double PercentageOfSessionSpentInZone4,
            double PercentageOfSessionSpentInZone5) GetTimeSpentInEachHrZone(int HeartRateDataCount, List<int> HeartRateDataList)
        {
            // get count of heart rates in each training zone
            var HrZoneValues = new List<string>();

            // loop through heart rate data and store 
            foreach (var HeartRateValue in HeartRateDataList)
            {
                int HrZoneEnumValue = GetHrZone(HeartRateValue);
                var HrZoneEnum = (HrZoneGetter.HrZone)HrZoneEnumValue;
                string HrZoneString = HrZoneEnum.ToString();
                HrZoneValues.Add(HrZoneString);
            }

            int Zone1Count = HrZoneValues.Count(x => x.Equals("Zone1"));
            int Zone2Count = HrZoneValues.Count(x => x.Equals("Zone2"));
            int Zone3Count = HrZoneValues.Count(x => x.Equals("Zone3"));
            int Zone4Count = HrZoneValues.Count(x => x.Equals("Zone4"));
            int Zone5Count = HrZoneValues.Count(x => x.Equals("Zone5"));

            // calculate % of time spent in each zone
            var PercentageOfSessionSpentInZone1 = Math.Round((double)Zone1Count / (double)HeartRateDataCount * 100, 1);
            var PercentageOfSessionSpentInZone2 = Math.Round((double)Zone2Count / (double)HeartRateDataCount * 100, 1);
            var PercentageOfSessionSpentInZone3 = Math.Round((double)Zone3Count / (double)HeartRateDataCount * 100, 1);
            var PercentageOfSessionSpentInZone4 = Math.Round((double)Zone4Count / (double)HeartRateDataCount * 100, 1);
            var PercentageOfSessionSpentInZone5 = Math.Round((double)Zone5Count / (double)HeartRateDataCount * 100, 1);

            return (PercentageOfSessionSpentInZone1,
                PercentageOfSessionSpentInZone2,
                PercentageOfSessionSpentInZone3,
                PercentageOfSessionSpentInZone4,
                PercentageOfSessionSpentInZone5);
        }

        private async void OnDelete_Clicked(System.Object sender, System.EventArgs e)
        {
            // confirm there is a workout to delete
            if (WorkoutItem == null)
            {
                return;
            }

            // confirm user's intention to delete
            if (!await DisplayAlert("Delete workout?", "This will permanently delete the workout. Are you sure you want to delete this workout?", "Yes", "Cancel"))
            {
                return;
            }

            Console.WriteLine($"Deleting workout with id: {WorkoutItem.Id}");

            try
            {
                WorkoutDatabase database = await WorkoutDatabase.Instance;
                await database.RemoveWorkoutAsync(WorkoutItem.Id);
                Console.WriteLine($"Deleted workout with id: {WorkoutItem.Id}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Deletion error", "Unable to delete workout. Please try again.", "Ok");
                Console.WriteLine($"Removal error: {ex.Message}");
            }

            // send user back to historical workouts page
            await Navigation.PopAsync();
        }

        // todo : calculate time spent in each HR zone (as opposed of % of time)

    }
}
