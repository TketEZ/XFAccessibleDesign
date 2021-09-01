using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PerformanceAI.Models;
using PerformanceAI.Services;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

// todo: cache previously loaded workouts

namespace PerformanceAI.Views
{
    public partial class OldWorkouts : ContentPage
    {
        public ObservableRangeCollection<WorkoutModel> HistoricalWorkouts { get; set; } = new ObservableRangeCollection<WorkoutModel>();

        private bool _hasNoPreviousSessions = false;
        public bool HasNoPreviousSessions
        {
            get => _hasNoPreviousSessions;
            set
            {
                _hasNoPreviousSessions = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoadingWorkouts
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public OldWorkouts()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            HasNoPreviousSessions = false;
            await LoadData();
        }

        private async Task LoadData()
        {

            if (IsLoadingWorkouts)
            {
                return;
            }

            try
            {
                IsLoadingWorkouts = true;
                HistoricalWorkouts.Clear();

                // load old workouts
                WorkoutDatabase database = await WorkoutDatabase.Instance;
                var historicalWorkouts = await database.GetAllWorkoutsAsync();

                if (historicalWorkouts != null && historicalWorkouts.Any())
                {
                    // update list
                    HistoricalWorkouts.AddRange(historicalWorkouts);
                }
                else
                {
                    HasNoPreviousSessions = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loading error: {ex.Message}");
                await DisplayAlert("Loading Error", "Unable to load historical workouts", "Ok");
            }
            finally
            {
                IsLoadingWorkouts = false;
            }
        }

        private async void ListView_ItemSelected(System.Object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            if (!(((ListView)sender).SelectedItem is WorkoutModel workout))
                return;

            await Navigation.PushAsync(new OldWorkoutDetailPage(workout));

            ((ListView)sender).SelectedItem = null;
        }

        private void ListView_ItemTapped(System.Object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }

    }
}
