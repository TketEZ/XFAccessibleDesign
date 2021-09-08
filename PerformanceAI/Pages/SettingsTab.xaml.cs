using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using PerformanceAI.Helpers;
using PerformanceAI.Services;
using PerformanceAI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PerformanceAI.Pages
{
    public partial class SettingsTab : ContentView, INotifyPropertyChanged
    {
        private bool IsPlayingSample = false;
        CancellationTokenSource _cts;

        IAudioSessionService audioService;

        public event PropertyChangedEventHandler MyPropertyChanged;
        void MyOnPropertyChanged([CallerMemberName] string name = "")
        {
            MyPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool _isIntervalAlertsToggled;
        public bool IsIntervalAlertsToggled
        {
            get
            {
                return _isIntervalAlertsToggled;
            }
            set
            {
                _isIntervalAlertsToggled = value;
                Settings.IsIntervalAlertsEnabled = value;
                MyOnPropertyChanged();
            }
        }

        private bool _isCurrentHrToggled;
        public bool IsCurrentHrToggled
        {
            get
            {
                return _isCurrentHrToggled;
            }
            set
            {
                _isCurrentHrToggled = value;
                Settings.IsCurrentHrAlertEnabled = value;
                MyOnPropertyChanged();
            }
        }

        private bool _isAvgHrToggled;
        public bool IsAvgHrToggled
        {
            get
            {
                return _isAvgHrToggled;
            }
            set
            {
                _isAvgHrToggled = value;
                Settings.isAvgHrEnabled = value;
                MyOnPropertyChanged();
            }
        }

        private bool _isMaxHrToggled;
        public bool IsMaxHrToggled
        {
            get
            {
                return _isMaxHrToggled;
            }
            set
            {
                _isMaxHrToggled = value;
                Settings.isMaxHrEnabled = value;
                MyOnPropertyChanged();
            }
        }

        private bool _isMinHrToggled;
        public bool IsMinHrToggled
        {
            get
            {
                return _isMinHrToggled;
            }
            set
            {
                _isMinHrToggled = value;
                Settings.isMinHrEnabled = value;
                MyOnPropertyChanged();
            }
        }

        private bool _isDurationToggled;
        public bool IsDurationToggled
        {
            get
            {
                return _isDurationToggled;
            }
            set
            {
                _isDurationToggled = value;
                Settings.isWorkoutDurationEnabled = value;
                MyOnPropertyChanged();
            }
        }

        private bool _isCurrentTimeToggled;
        public bool IsCurrentTimeToggled
        {
            get
            {
                return _isCurrentTimeToggled;
            }
            set
            {
                _isCurrentTimeToggled = value;
                Settings.isCurrentTimeEnabled = value;
                MyOnPropertyChanged();
            }
        }

        private bool _isMaxHrAlarmEnabled;
        public bool IsMaxHrAlarmEnabled
        {
            get
            {
                return _isMaxHrAlarmEnabled;
            }
            set
            {
                _isMaxHrAlarmEnabled = value;
                Settings.isMaxHrAlarmEnabled = value;
                MyOnPropertyChanged();
            }
        }

        private bool _isHrZonesAlarmEnabled;
        public bool IsHrZonesAlarmEnabled
        {
            get
            {
                return _isHrZonesAlarmEnabled;
            }
            set
            {
                _isHrZonesAlarmEnabled = value;
                Settings.IsHrZoneChangeAlertEnabled = value;
                MyOnPropertyChanged();
            }
        }

        private bool _isOnDemandDataToggled;
        public bool IsOnDemandDataToggled
        {
            get
            {
                return _isOnDemandDataToggled;
            }
            set
            {
                _isOnDemandDataToggled = value;
                Settings.IsOnDemandDataenabled = value;
                MyOnPropertyChanged();
            }
        }

        public SettingsTab()
        {
            InitializeComponent();
            BindingContext = this;

            // not needed on Android API 26, only IOS
            // https://stackoverflow.com/questions/49979619/xamarin-forms-dependencyservice-not-for-all-platforms
            if (Device.RuntimePlatform == Device.iOS)
            {
                audioService = DependencyService.Resolve<IAudioSessionService>();
            }

            IntervalAlertsSwitch.IsToggled = Settings.IsIntervalAlertsEnabled;
            IntervalFrequencyPicker.SelectedItem = Settings.IntervalFrequency.ToString();

            CurrentHrSwitch.IsToggled = Settings.IsCurrentHrAlertEnabled;
            AvgHrSwitch.IsToggled = Settings.isAvgHrEnabled;
            MaxHrSwitch.IsToggled = Settings.isMaxHrEnabled;
            MinHrSwitch.IsToggled = Settings.isMinHrEnabled;
            DurationSwitch.IsToggled = Settings.isWorkoutDurationEnabled;
            CurrentTimeSwitch.IsToggled = Settings.isCurrentTimeEnabled;

            //audio
            OnDemandDataSwitch.IsToggled = Settings.IsOnDemandDataenabled;

            // Alarms
            MaxHrAlertSwitch.IsToggled = Settings.isMaxHrAlarmEnabled;
            HRZonesSwitch.IsToggled = Settings.IsHrZoneChangeAlertEnabled;

            // volume
            VolumeSlider.Value = Settings.VolumeValue;

        }

        void SampleBtn_Clicked(System.Object sender, System.EventArgs e)
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

            if (CurrentHrSwitch.IsToggled)
            {
                SampleCallout = SampleCallout + "Current heart rate: 85 bpm. ";
            }

            if (AvgHrSwitch.IsToggled)
            {
                SampleCallout = SampleCallout + "Average heart rate: 90 bpm. ";
            }

            if (MaxHrSwitch.IsToggled)
            {
                SampleCallout = SampleCallout + "Maximum heart rate: 110 bpm. ";
            }

            if (MinHrSwitch.IsToggled)
            {
                SampleCallout = SampleCallout + "Minimum heart rate: 80 bpm. ";
            }

            if (DurationSwitch.IsToggled)
            {
                SampleCallout = SampleCallout + "Workout Duration: 5 minutes. ";
            }

            if (CurrentTimeSwitch.IsToggled)
            {
                SampleCallout = SampleCallout + "Current time: " + DateTime.Now.ToString("HH:mm") + ". ";
            }

            // if all switches are off
            if (CurrentHrSwitch.IsToggled != true &&
                AvgHrSwitch.IsToggled != true &&
                MaxHrSwitch.IsToggled != true &&
                MinHrSwitch.IsToggled != true &&
                DurationSwitch.IsToggled != true &&
                CurrentTimeSwitch.IsToggled != true)
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

        void VolumeSlider_ValueChanged(System.Object sender, Xamarin.Forms.ValueChangedEventArgs e)
        {
            double _volume = e.NewValue;

            Settings.VolumeValue = _volume;
        }

        void IntervalFrequencyPicker_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            int _intervalFrequency = Convert.ToInt32(IntervalFrequencyPicker.SelectedItem);
            Settings.IntervalFrequency = _intervalFrequency;
        }

        async void ConfigureHrZones_Tapped(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new HrZones());
        }

        //void IntervalAlertsSwitch_Toggled(System.Object sender, Xamarin.Forms.ToggledEventArgs e)
        //{
        //    if (sender is Switch)
        //    {
        //        Switch ToggledSwitch = (Switch)sender;
        //        Settings.
        //        Console.WriteLine($"Switch state: {ToggledSwitch.IsToggled}");
        //        Console.WriteLine($"Switch state: {IsIntervalAlertsToggled}");
        //        // use this one
        //        Console.WriteLine($"Switch state: {Settings.IsIntervalAlertsEnabled}");
        //        Console.WriteLine($"Switch state: {ToggledSwitch.Id}");
        //        // cc10b4c0-35b3-4b07-887f-4a9a2ed60b34
        //    }
        //}
    }
}
