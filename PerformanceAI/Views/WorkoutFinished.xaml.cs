using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using PerformanceAI.Models;
using PerformanceAI.Services;
using Xamarin.Forms;

namespace PerformanceAI.Views
{
    public partial class WorkoutFinished : ContentPage, INotifyPropertyChanged
    {
        private int _completedWorkoutId;

        private string _finishDate;
        public string FinishDate
        {
            get
            {
                return _finishDate;
            }
            set
            {
                if (value != _finishDate)
                {
                    _finishDate = value;
                    OnPropertyChanged(nameof(FinishDate));
                }
            }
        }

        private string _finishTime;
        public string FinishTime
        {
            get
            {
                return _finishTime;
            }
            set
            {
                if (value != _finishTime)
                {
                    _finishTime = value;
                    OnPropertyChanged(nameof(FinishTime));
                }
            }
        }

        private string _sessionDuration;
        public string SessionDuration
        {
            get
            {
                return _sessionDuration;
            }
            set
            {
                if (value != _sessionDuration)
                {
                    _sessionDuration = value;
                    OnPropertyChanged(nameof(_sessionDuration));
                }
            }
        }

        private int _sessionAvgHeartRate;
        public int SessionAvgHeartRate
        {
            get
            {
                return _sessionAvgHeartRate;
            }
            set
            {
                if (value != _sessionAvgHeartRate)
                {
                    _sessionAvgHeartRate = value;
                    OnPropertyChanged(nameof(SessionAvgHeartRate));
                }
            }
        }

        private int _sessionMaxHeartRate;
        public int SessionMaxHeartRate
        {
            get
            {
                return _sessionMaxHeartRate;
            }
            set
            {
                if (value != _sessionMaxHeartRate)
                {
                    _sessionMaxHeartRate = value;
                    OnPropertyChanged(nameof(SessionMaxHeartRate));
                }
            }
        }

        private int _sessionMinHeartRate;
        public int SessionMinHeartRate
        {
            get
            {
                return _sessionMinHeartRate;
            }
            set
            {
                if (value != _sessionMinHeartRate)
                {
                    _sessionMinHeartRate = value;
                    OnPropertyChanged(nameof(SessionMinHeartRate));
                }
            }
        }

        public WorkoutFinished(int CompletedWorkoutId)
        {
            InitializeComponent();
            BindingContext = this;
            NavigationPage.SetHasBackButton(this, false); //https://stackoverflow.com/questions/24935929/xamarin-forms-getting-rid-of-back-button-in-nav-bar
            _completedWorkoutId = CompletedWorkoutId;
            Console.WriteLine($"Id of workout that just finished: {_completedWorkoutId}");
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await LoadFinishedWorkoutData();

            // todo: notify user workout finished
            // todo: make IsSpeaking an app-level property so it canbe used on all different pages 
            //string WorkoutFinishedCallout = "Workout finished";
            //CallOutMetrics(WorkoutFinishedCallout);

        }

        private async Task LoadFinishedWorkoutData()
        {

            WorkoutDatabase database = await WorkoutDatabase.Instance;
            WorkoutModel FinishedWorkout = await database.GetWorkoutAsync(_completedWorkoutId);

            if (FinishedWorkout != null)
            {

                TimeSpan workoutDurationTime = TimeSpan.FromTicks(FinishedWorkout.WorkoutDurationInTicks);

                // todo: double check start time and end times are accurate
                _finishDate = FinishedWorkout.ShortDateListView;
                _finishTime = FinishedWorkout.ShortTimeListView;
                _sessionAvgHeartRate = FinishedWorkout.SessionAvgHr;
                _sessionMaxHeartRate = FinishedWorkout.SessionMaxHr;
                _sessionMinHeartRate = FinishedWorkout.SessionMinHr;
                _sessionDuration = string.Format("{0:00}:{1:00}:{2:00}", workoutDurationTime.Hours, workoutDurationTime.Minutes, workoutDurationTime.Seconds);


                FinishDateLbl.Text = $"Workout completed on: {_finishDate}";
                FinishTimeLbl.Text = $"Workout finished at: {_finishTime}";
                SessionDurationLbl.Text = $"Workout duration: {_sessionDuration}";
                SessionMaxHrLbl.Text = $"Highest heart rate: {_sessionMaxHeartRate.ToString()}";
                SessionAvgHrLbl.Text = $"Average heart rate: {_sessionAvgHeartRate.ToString()}";
                SessionMinHrLbl.Text = $"Lowest heart rate: {_sessionMinHeartRate.ToString()}";

            }
        }

        private async void OldWorkoutsBtn_Clicked(System.Object sender, System.EventArgs e)
        {
            await Navigation.PopToRootAsync();
        }

        //protected override bool OnBackButtonPressed()
        //{
        //    return;
        //}

        //public event PropertyChangedEventHandler PropertyChanged;
        //protected virtual void OnPropertyChanged(string propertyName)
        //{
        //    if (PropertyChanged != null {
        //        PropertyChanged(this,
        //            new PropertyChangedEventArgs(propertyName));
        //    }
        //}

        //async Task SaveWorkoutBtn_Clicked(System.Object sender, System.EventArgs e)
        //{
        //    // todo: button to save workout to cloud
        //}

        // todo: button to delete workout instance
        //void DiscardWorkoutBtn_Clicked(System.Object sender, System.EventArgs e)
        //{
        //    //var CompletedWorkout = (Workout)BindingContext;
        //    //todo: get rid of CompletedWorkout
        //    //todo: Handle navigation
        //    //await Navigation.PopAsync();
        //}

        //todo: Automatically hear summary of key stats.The metrics included in the summary should be configurable in settings. Should have a button which allows you to repeat summary. 

    }
}
