using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PerformanceAI.Models;
using PerformanceAI.Services;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace PerformanceAI.Views
{
    public partial class TrainingZoneList : ContentPage
    {
        public ObservableRangeCollection<TrainingZoneModel> TrainingZoneCollection { get; set; } = new ObservableRangeCollection<TrainingZoneModel>();

        private bool _isLoadingZones;
        public bool IsLoadingZones
        {
            get => _isLoadingZones;
            set
            {
                _isLoadingZones = value;
                OnPropertyChanged();
            }
        }

        public TrainingZoneList()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadTrainingZoneData();
        }

        private async Task LoadTrainingZoneData()
        {

            if (IsLoadingZones)
            {
                return; 
            }

            TrainingZoneCollection.Clear();
            IsLoadingZones = true;

            try
            {
                WorkoutDatabase database = await WorkoutDatabase.Instance;
                var trainingZones = await database.GetAllTrainingZonesAsync();

                if(trainingZones != null && trainingZones.Any())
                {
                    TrainingZoneCollection.AddRange(trainingZones);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Loading error: {ex.Message}");
                await DisplayAlert("Loading Error", "Unable to load training zones", "Ok");
            }
            finally
            {
                IsLoadingZones = false;
            }
        }

        async void OnItemAdded(System.Object sender, System.EventArgs e)
        {
            var FreshTrainingZone = new TrainingZoneModel();

            await Navigation.PushAsync(new TrainingZoneDetailPage(FreshTrainingZone));
        }

        async void TrainingZonesListView_ItemSelected(System.Object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {

            if (!(((ListView)sender).SelectedItem is TrainingZoneModel trainingZone))
                return;

            await Navigation.PushAsync(new TrainingZoneDetailPage(trainingZone));

            ((ListView)sender).SelectedItem = null;
        }

        void TrainingZonesListView_ItemTapped(System.Object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            ((ListView)sender).SelectedItem = null;
        }
    }
}
