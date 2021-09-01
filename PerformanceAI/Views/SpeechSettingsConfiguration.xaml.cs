using System;
using System.Collections.Generic;
using System.Threading;
using PerformanceAI.Helpers;
using PerformanceAI.Services;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PerformanceAI.Views
{
    public partial class SpeechSettingsConfiguration : ContentPage
    {

        private IDevice _connectedDevice;
        private IAdapter _adapter;

        private bool IsPlayingSample = false;
        CancellationTokenSource _cts;

        private IService _batteryService;
        private ICharacteristic _batteryCharacteristic;

        IAudioSessionService audioService;

        public SpeechSettingsConfiguration(IDevice ConnectedDevice, IAdapter adapter)
        {
            InitializeComponent();
            BindingContext = this;

            // not needed on Android API 26, only IOS
            // https://stackoverflow.com/questions/49979619/xamarin-forms-dependencyservice-not-for-all-platforms
            if (Device.RuntimePlatform == Device.iOS)
            {
                audioService = DependencyService.Resolve<IAudioSessionService>();
            }

            _connectedDevice = ConnectedDevice;
            _adapter = adapter;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            IntervalAlertsSwitch.On = Settings.IsIntervalAlertsEnabled;
            IntervalFrequencyPicker.SelectedItem = Settings.IntervalFrequency.ToString();

            // metric switches
            CurrentHrSwitch.On = Settings.IsCurrentHrAlertEnabled;
            AvgHrSwitch.On = Settings.isAvgHrEnabled;
            MaxHrSwitch.On = Settings.isMaxHrEnabled;
            MinHrSwitch.On = Settings.isMinHrEnabled;
            DurationSwitch.On = Settings.isWorkoutDurationEnabled;
            CurrentTimeSwitch.On = Settings.isCurrentTimeEnabled;

            //audio
            OnDemandDataSwitch.On = Settings.IsOnDemandDataenabled;

            // Alarms
            MaxHrAlertSwitch.On = Settings.isMaxHrAlarmEnabled;
            HRZonesSwitch.On = Settings.IsHrZoneChangeAlertEnabled;
            //MinHrAlarmSwitch.On = Settings.isMinHrAlarmEnabled;
            //MaxHrAlarmSwitch.On = Settings.isMaxHrAlarmEnabled;
            //MinHrAlarmValuePicker.SelectedItem = Settings.MinHrAlarmValue.ToString();
            //MaxHrAlarmValuePicker.SelectedItem = Settings.MaxHrAlarmValue.ToString();

            // set device battery info
            string batteryLevel;
            _batteryService = await _connectedDevice.GetServiceAsync(HeartRateIdentifiers.BatteryService);

            if (_batteryService != null)
            {
                _batteryCharacteristic = await _batteryService.GetCharacteristicAsync(HeartRateIdentifiers.BatteryLevelCharacteristics);
            }

            if (_batteryCharacteristic != null)
            {
                var BatteryLevelBytes = await _batteryCharacteristic?.ReadAsync();
                batteryLevel = BatteryLevelBytes[0].ToString();
                ConnectedSensorBatteryLevel.Text = batteryLevel + "%";
            }

            // device info
            ConnectedSensorName.Text = _connectedDevice.Name;
            ConnectedSensorId.Text = _connectedDevice.Id.ToString();

            // volume
            VolumeSlider.Value = Settings.VolumeValue;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // cancel sample audio if still playing
            if (IsPlayingSample)
            {
                CancelSampleAudio();
            }

            Settings.IsIntervalAlertsEnabled = IntervalAlertsSwitch.On;

            // metric switches
            Settings.IsCurrentHrAlertEnabled = CurrentHrSwitch.On;
            Settings.isAvgHrEnabled = AvgHrSwitch.On;
            Settings.isMaxHrEnabled = MaxHrSwitch.On;
            Settings.isMinHrEnabled = MinHrSwitch.On;
            Settings.isWorkoutDurationEnabled = DurationSwitch.On;
            Settings.isCurrentTimeEnabled = CurrentTimeSwitch.On;

            //audio 
            Settings.IsOnDemandDataenabled = OnDemandDataSwitch.On;

            // alarms
            Settings.isMaxHrAlarmEnabled = MaxHrAlertSwitch.On;
            Settings.IsHrZoneChangeAlertEnabled = HRZonesSwitch.On;
            //Settings.isMinHrAlarmEnabled = MinHrAlarmSwitch.On;
            //Settings.isMaxHrAlarmEnabled = MaxHrAlarmSwitch.On;

            // device info
            Settings.SensorName = _connectedDevice.Name;
            Settings.SensorId = _connectedDevice.Id.ToString();
        }

        private void SampleBtn_Clicked(System.Object sender, System.EventArgs e)
        {
            if (IsPlayingSample)
            {
                CancelSampleAudio();
            }
            else
            {
                playSampleAudio();
            }
        }

        private async void playSampleAudio()
        {
            IsPlayingSample = true;
            updateUI();

            // if cancellation token exists, get rid
            if (_cts?.IsCancellationRequested is false)
            {
                _cts.Cancel();
            }

            // create new cancellation token
            _cts = new CancellationTokenSource();
            if (_cts != null)
            {
                Console.WriteLine("Cancellation token created for sample audio!");
            }

            string SampleAudioText = CreateSampleText();

            var SpeechSettings = new SpeechOptions()
            {
                Volume = (float)VolumeSlider.Value
            };

            try
            {
                Console.WriteLine("Playing sample audio");
                audioService?.ActivateAudioPlaybackSession();
                await TextToSpeech.SpeakAsync(SampleAudioText, SpeechSettings, cancelToken: _cts.Token);
            }
            catch (Exception ex)
            {
                await DisplayAlert("TTS Error", "Text-to-speech is not supported on this device.", "Ok");
                Console.WriteLine($"Text to speech error: {ex.Message}");
            }
            finally
            {
                IsPlayingSample = false;
                updateUI();
            }
        }

        private string CreateSampleText()
        {
            string SampleCallout = "";

            if (CurrentHrSwitch.On)
            {
                SampleCallout = SampleCallout + "Current heart rate: 85 bpm. ";
            }

            if (AvgHrSwitch.On)
            {
                SampleCallout = SampleCallout + "Average heart rate: 90 bpm. ";
            }

            if (MaxHrSwitch.On)
            {
                SampleCallout = SampleCallout + "Maximum heart rate: 110 bpm. ";
            }

            if (MinHrSwitch.On)
            {
                SampleCallout = SampleCallout + "Minimum heart rate: 80 bpm. ";
            }

            if (DurationSwitch.On)
            {
                SampleCallout = SampleCallout + "Workout Duration: 5 minutes. ";
            }

            if (CurrentTimeSwitch.On)
            {
                SampleCallout = SampleCallout + "Current time: " + DateTime.Now.ToString("HH:mm") + ". ";
            }

            // if all switches are off
            if (CurrentHrSwitch.On != true &&
                AvgHrSwitch.On != true &&
                MaxHrSwitch.On != true &&
                MinHrSwitch.On != true &&
                DurationSwitch.On != true &&
                CurrentTimeSwitch.On != true)
            {
                SampleCallout = "Current heart rate: 85 bpm. " + "Current time: " + DateTime.Now.ToString("HH:mm") + ". ";
            }

            return SampleCallout;

        }

        private void CancelSampleAudio()
        {
            // if cancellation not requested (i.e. false) then return
            if (_cts?.IsCancellationRequested ?? true) //https://stackoverflow.com/questions/37851873/what-does-mean-after-variable-in-c/37852031
            {
                return;
            }
            else
            {
                Console.WriteLine("Sample audio cancelled");
                _cts.Cancel();
                IsPlayingSample = false;
                updateUI();
            }
        }

        private void updateUI()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (IsPlayingSample == true)
                {
                    SampleBtn.Text = "Stop playing";
                }
                else
                {
                    SampleBtn.Text = "Play sample audio";
                }
            });
        }

        void IntervalFrequencyPicker_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            int _intervalFrequency = Convert.ToInt32(IntervalFrequencyPicker.SelectedItem);

            Settings.IntervalFrequency = _intervalFrequency;
        }

        async void FinishedConfigBtn_Clicked(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new Workout(_connectedDevice, _adapter));
        }

        //void MinHrAlarmValue_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        //{
        //    int _minHrAlarmValue = Convert.ToInt32(MinHrAlarmValuePicker.SelectedItem);

        //    Settings.MinHrAlarmValue = _minHrAlarmValue;
        //}

        //void MaxHrAlarmValue_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        //{
        //    int _maxHrAlarmValue = Convert.ToInt32(MaxHrAlarmValuePicker.SelectedItem);

        //    Settings.MaxHrAlarmValue = _maxHrAlarmValue;
        //}

        void VolumeSlider_ValueChanged(System.Object sender, Xamarin.Forms.ValueChangedEventArgs e)
        {
            double _volume = e.NewValue;

            Settings.VolumeValue = _volume;
        }

        async void ConfigureHrZones_Tapped(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new HrZones());
        }

        async void TrainingStudio_Tapped(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new TrainingZoneList());
        }
    }
}
