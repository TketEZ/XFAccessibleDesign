using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.CognitiveServices.Speech;
using PerformanceAI.Helpers;
using PerformanceAI.Models;
using PerformanceAI.Services;
using PerformanceAI.Utils;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Essentials;
using Xamarin.Forms;
using Timer = System.Timers.Timer;

namespace PerformanceAI.Views
{
    public partial class Workout : ContentPage, INotifyPropertyChanged
    {
        private IDevice _connectedDevice;
        private IAdapter _adapter;
        private string _deviceName = "";
        private Guid _deviceId;

        private IService _hrService;
        private ICharacteristic _hrCharacteristic;

        private double _totalHeartRateReadings;
        private double _numberOfHeartRateReadings;

        private Timer _sessionUiTimer;
        int hours = 0, minutes = 0, seconds = 0, milliseconds = 0;

        private Stopwatch _sessionDurationTimer;

        private Timer _audioAnnouncementsTimer;
        private bool IsPlayingAudio = false;
        CancellationTokenSource _cts;

        private long elapsedTicks;
        private DateTime _workoutStartTime;
        private DateTime _workoutEndTime;
        private long _workoutDuration;

        private WorkoutModel _currentWorkoutValues;

        SpeechRecognizer recognizer;
        IMicrophoneService micService;
        bool _isDetectingSpeech = false;

        IAudioSessionService audioService;
        bool isSpeaking;
        bool IsSpeechRecognitionCancelled = false;


        private string _heartRate = "Heart Rate";
        public string HeartRateLbl
        {
            get
            {
                return _heartRate;
            }
            set
            {
                _heartRate = value;
                OnPropertyChanged();
            }
        }

        private string _heartRateAvg = "Average heart rate";
        public string HeartRateAvgLbl
        {
            get
            {
                return _heartRateAvg;
            }
            set
            {
                _heartRateAvg = value;
                OnPropertyChanged();
            }
        }

        private string _batteryLevel = "Battery Level";
        public string BatteryLevel
        {
            get
            {
                return _batteryLevel;
            }
            set
            {
                _batteryLevel = value;
                OnPropertyChanged();
            }
        }

        private bool _isConnecting;
        private bool _isConnected;
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (IsPlayingAudio)
            {
                StopAudioAnnouncements();
            }
        }

        public Workout(IDevice ConnectedDevice, IAdapter adapter)
        {
            InitializeComponent();

            BindingContext = this;

            if (ConnectedDevice != null)
            {
                IsConnected = true;
                Console.WriteLine($"Connected device is {ConnectedDevice.Name}");
            }

            micService = DependencyService.Resolve<IMicrophoneService>();

            // not needed on Android API 26, only IOS
            // https://stackoverflow.com/questions/49979619/xamarin-forms-dependencyservice-not-for-all-platforms
            if (Device.RuntimePlatform == Device.iOS)
            {
                audioService = DependencyService.Resolve<IAudioSessionService>();
            }

            _adapter = adapter;
            _connectedDevice = ConnectedDevice;
            _deviceName = ConnectedDevice.Name;
            _deviceId = ConnectedDevice.Id;

        }

        // todo: handle on dissapearing
        // protected override 

        async void StartWorkout_Clicked(System.Object sender, EventArgs e)
        {

            if (_isConnected)
            {

                Console.WriteLine("Workout started");

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StopWorkoutButton.IsVisible = true;
                });

                // todo: move this connection stuff to new workout page
                _hrService = await _connectedDevice.GetServiceAsync(HeartRateIdentifiers.HeartRateService);
                Console.WriteLine($"Heart Rate Service: {_hrService}");
                _hrCharacteristic = await _hrService.GetCharacteristicAsync(HeartRateIdentifiers.HeartRateCharacteristic);
                Console.WriteLine($"Heart Rate Characteristics: {_hrCharacteristic}");

                if (_hrCharacteristic != null)
                {
                    _hrCharacteristic.ValueUpdated += _hrCharacteristic_ValueUpdated;
                    await _hrCharacteristic.StartUpdatesAsync();
                }
                // up till here

                //string WorkoutStartedCallout = "Workout started";
                //CallOutMetrics(WorkoutStartedCallout);

                SetUiTimer();
                SetSessionDurationTimer();

                if (Settings.IsIntervalAlertsEnabled && Settings.IntervalFrequency > 0)
                {
                    SetAnnouncementsTimer();
                }

                if (Settings.IsOnDemandDataenabled == true)
                {
                    Console.WriteLine("OnDemandData function triggered: " + Settings.IsOnDemandDataenabled);
                    OnDemandData();
                }
            }
        }

        private void SetSessionDurationTimer()
        {
            // initialise stopwatch for calculating workout duration
            _sessionDurationTimer = new Stopwatch();
            _sessionDurationTimer.Start();
        }

        private void SetUiTimer()
        {
            _sessionUiTimer = new Timer();
            _sessionUiTimer.Interval = 1;
            _sessionUiTimer.Elapsed += _sessionUiTimer_Elapsed;
            _sessionUiTimer.AutoReset = true;
            _sessionUiTimer.Start();
        }

        private void SetAnnouncementsTimer()
        {
            // retrieve interval frequency as miliseconds
            TimeSpan CalloutFreq = TimeSpan.FromSeconds(Settings.IntervalFrequency);
            double CalloutFreqInMiliseconds = CalloutFreq.TotalMilliseconds;

            _audioAnnouncementsTimer = new Timer(CalloutFreqInMiliseconds);
            _audioAnnouncementsTimer.Elapsed += _audioAnnouncementsTimer_Elapsed;
            _audioAnnouncementsTimer.AutoReset = true;
            _audioAnnouncementsTimer.Enabled = true;
        }

        private void _audioAnnouncementsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var Callouts = "";

            if (Settings.IsCurrentHrAlertEnabled)
            {
                Callouts = Callouts + " Current heart rate: " + HeartRateLbl + "bpm. ";
            }

            if (Settings.isAvgHrEnabled)
            {
                Callouts = Callouts + "Average heart rate:" + HeartRateAvgLbl + "bpm. ";
            }

            if (Settings.isMaxHrEnabled)
            {
                Callouts = Callouts + "Session's Maximum heart rate:" + _sessionHeartRateMax.ToString() + "bpm. ";
            }

            if (Settings.isMinHrEnabled)
            {
                Callouts = Callouts + "Session's Minimum heart rate:" + _sessionHeartRateMin.ToString() + "bpm. ";
            }

            if (Settings.isWorkoutDurationEnabled)
            {
                string CurrentWorkoutTime = GetCurrentWorkoutDuration();

                Callouts = Callouts + CurrentWorkoutTime;
            }

            if (Settings.isCurrentTimeEnabled)
            {
                Callouts = Callouts + "Current time: " + DateTime.Now.ToString("HH:mm") + ". ";
            }

            CallOutMetrics(Callouts); // speak out requested callouts every intervalFrequency seconds
        }

        private string GetCurrentWorkoutDuration()
        {

            var CurrentWorkoutDuration = "";

            // if first minute of workout, 
            if ( seconds <= 59 && minutes == 0 && hours == 0)
            {
                //// if workout duration so far is greater than interval 
                //if (seconds >= Settings.IntervalFrequency)
                //{
                    CurrentWorkoutDuration += "Workout duration:";

                    CurrentWorkoutDuration += $"{seconds.ToString()} seconds. ";
                //}
            }
            else // else, just add minutes and hours 
            {
                CurrentWorkoutDuration += "Workout duration:";

                if (hours > 0)
                {
                    CurrentWorkoutDuration += $"{hours.ToString()} hours and ";

                    if (minutes > 0)
                    {
                        CurrentWorkoutDuration += $"{minutes.ToString()} minutes. ";
                    }
                }
                else if (minutes > 1)
                {
                    CurrentWorkoutDuration += $"{minutes.ToString()} minutes. ";
                }
                else
                {
                    CurrentWorkoutDuration += $"{minutes.ToString()} minute. ";
                }
            }

            return CurrentWorkoutDuration;
        }

        private void _sessionUiTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IsFirstTimeReadingSensor)
                return;

            milliseconds++;
            if (milliseconds >= 1000)
            {
                seconds++;
                milliseconds = 0;
            }

            if (seconds == 59)
            {
                minutes++;
                seconds = 0;

            }
            if (minutes == 59)
            {
                hours++;
                minutes = 0;
            }
            if (hours == 59 && minutes == 59 && seconds == 59)
            {
                hours = 0;
                minutes = 0;
                seconds = 0;
            }


            MainThread.BeginInvokeOnMainThread(() =>
            {
                TimerLabel.Text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            });
        }

        private async void CallOutMetrics(string callouts)
        {
            if (!IsPlayingAudio && !HasDisabledAudioAnnouncements)
            {
                await SpeakCallouts(callouts);
            }
        }

        private async Task SpeakCallouts(string calloutString)
        {
            IsPlayingAudio = true;

            // if cancellation token exists, get rid
            if (_cts?.IsCancellationRequested is false)
            {
                _cts.Cancel();
            }

            _cts = new CancellationTokenSource();
            if (_cts != null)
            {
                Console.WriteLine("Cancellation token created!");
            }

            var SpeechSettings = new SpeechOptions()
            {
                //Locale = (Locale)chosenLocale,
                Volume = (float)Settings.VolumeValue,
            };

            // if user has enabled on-demand data, then switch off STT before starting TTS
            if (Settings.IsOnDemandDataenabled == true && _isDetectingSpeech)
            {
                await StopSpeechRecognition();
            }

            // TTS
            try
            {
                Console.WriteLine("TTS Working");
                audioService?.ActivateAudioPlaybackSession();
                await TextToSpeech.SpeakAsync(calloutString, SpeechSettings, cancelToken: _cts.Token);
                Console.WriteLine("TTS Finished");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Audio announcement Error", "Audio announcements unavailable because text-to-speech is not supported on this device.", "Ok");
                Console.WriteLine($"Text to speech error: {ex.Message}");
            }
            finally
            {
                IsPlayingAudio = false;
            }

            // Resume STT now TTS has finished
            if (IsSpeechRecognitionCancelled == false &&
                Settings.IsOnDemandDataenabled == true)
            {
                await StartSpeechRecognition();
            }

        }

        int _sessionHeartRateMax;
        int _sessionHeartRateMin;
        int _sessionHeartRateAvg;
        List<int> _heartRateList;
        bool IsFirstTimeReadingSensor = true;
        private void _hrCharacteristic_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
        {

            if (isDisconnecting)
            {
                return;
            }

            try
            {

                int heartRateVal = 0;
                var HeartRatebytes = e.Characteristic.Value;
                heartRateVal = HeartRatebytes[1];
                Console.WriteLine($"Heart Rate reading: {heartRateVal}");

                if (IsFirstTimeReadingSensor)
                {
                    HeartRateLbl = "---";
                    HeartRateAvgLbl = "---";
                    BatteryLevel = "---";
                    TimerLabel.Text = "---";
                    elapsedTicks = 0;
                    _sessionHeartRateMax = 0;
                    _sessionHeartRateMin = 300;
                    _totalHeartRateReadings = 0;
                    _numberOfHeartRateReadings = 0;
                    _workoutStartTime = DateTime.UtcNow;
                    _workoutEndTime = DateTime.UtcNow;
                    _workoutDuration = 0;
                    _heartRateList = new List<int>();
                    IsFirstTimeReadingSensor = false;
                    
                } else if (heartRateVal > 0)
                {

                    // display heart rate to user
                    HeartRateLbl = heartRateVal.ToString();

                    // store heart rate
                    _heartRateList.Add(heartRateVal);

                    // find session's max heart rate
                    if (heartRateVal > _sessionHeartRateMax)
                    {
                        _sessionHeartRateMax = heartRateVal;
                    }

                    // find session's min heart rate
                    if (heartRateVal < _sessionHeartRateMin)
                    {
                        _sessionHeartRateMin = heartRateVal;
                    }

                    // calculate session's average heart rate
                    _totalHeartRateReadings = _totalHeartRateReadings + heartRateVal;
                    _numberOfHeartRateReadings = _numberOfHeartRateReadings + 1;
                    if (_totalHeartRateReadings > 0 && _numberOfHeartRateReadings > 0)
                    {
                        _sessionHeartRateAvg = (int)Math.Round(_totalHeartRateReadings / _numberOfHeartRateReadings, 0);

                        HeartRateAvgLbl = _sessionHeartRateAvg.ToString();
                    }

                    if (Settings.isMaxHrAlarmEnabled)
                    {
                        AlertIfExceededUserMaxHr(heartRateVal);
                    }

                    if (Settings.IsHrZoneChangeAlertEnabled)
                    {
                        AlertIfChangedHrZone(heartRateVal);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reading sensor error: {ex.Message}");
            }
        }

        int Zone1LowerBound = Settings.HrZone1LowerBound;
        int Zone1UpperBound = Settings.HrZone1UpperBound;
        int Zone2UpperBound = Settings.HrZone2UpperBound;
        int Zone3UpperBound = Settings.HrZone3UpperBound;
        int Zone4UpperBound = Settings.HrZone4UpperBound;
        int Zone5UpperBound = Settings.MaxHr;
        int HrZoneState;
        private void AlertIfChangedHrZone(int CurrentHrVal)
        {
            // do nothing if hr below any of prescribed zones 
            if (CurrentHrVal < Zone1LowerBound)
            {
                return;
            }

            //if in zone 1 
            if (CurrentHrVal >= Zone1LowerBound && CurrentHrVal <= Zone1UpperBound && HrZoneState != 1)
            {
                HrZoneState = 1;
                string Callout_zone1 = "Zone 1";
                CallOutMetrics(Callout_zone1);
            }

            // if in zone 2 
            if (CurrentHrVal > Zone1UpperBound && CurrentHrVal <= Zone2UpperBound && HrZoneState != 2)
            {
                HrZoneState = 2;
                string Callout_zone2 = "Zone 2";
                CallOutMetrics(Callout_zone2);
            }

            // if in zone 3 
            if (CurrentHrVal > Zone2UpperBound && CurrentHrVal <= Zone3UpperBound && HrZoneState != 3)
            {
                HrZoneState = 3;
                string Callout_zone3 = "Zone 3";
                CallOutMetrics(Callout_zone3);
            }

            // if in zone 4 
            if (CurrentHrVal > Zone3UpperBound && CurrentHrVal <= Zone4UpperBound && HrZoneState != 4)
            {
                HrZoneState = 4;
                string Callout_zone4 = "Zone 4";
                CallOutMetrics(Callout_zone4);
            }

            // if in zone 5 
            if (CurrentHrVal > Zone4UpperBound && CurrentHrVal <= maxHrValue && HrZoneState != 5)
            {
                HrZoneState = 5;
                string Callout_zone5 = "Zone 5";
                CallOutMetrics(Callout_zone5);
            }
        }

        // https://stackoverflow.com/questions/28807511/run-a-function-only-once-when-a-certain-condition-is-met-but-rest-of-another-con/28807685
        int MaxHrState;
        int maxHrValue = Settings.MaxHr;
        private void AlertIfExceededUserMaxHr(int CurrentHrVal)
        {
            if (CurrentHrVal > maxHrValue && MaxHrState != 1)
            {
                // set state so callout is only called out once when user exceeds max threshold
                MaxHrState = 1;
                string Callout_exceededMaxHr = "Warning! Exceeded Max Heart rate";
                CallOutMetrics(Callout_exceededMaxHr);
            }
            else if (CurrentHrVal < maxHrValue && MaxHrState != 2)
            {
                // if user is currently over max threshold
                if (MaxHrState == 1)
                {
                    MaxHrState = 2;
                    string Callout_fellBackBelowMax = "Back in safe zone";
                    CallOutMetrics(Callout_fellBackBelowMax);
                }
            }
        }

        private async void StopWorkoutButton_Clicked(System.Object sender, System.EventArgs e)
        {
            if (_isConnecting || _isConnected)
            {
                //todo: add context action: hold to confirm finished workout?
                Console.WriteLine("Workout stopped");

                // now workout finished, calculate duration of workout
                _sessionDurationTimer.Stop();
                TimeSpan WorkoutDuration = _sessionDurationTimer.Elapsed;
                _sessionDurationTimer = null;

                // stop UI Timer
                _sessionUiTimer.Stop();
                _sessionUiTimer = null;

                // stop announcements
                if (Settings.IsIntervalAlertsEnabled)
                {
                    _audioAnnouncementsTimer.Stop();
                    _audioAnnouncementsTimer.Elapsed -= _audioAnnouncementsTimer_Elapsed;
                    _audioAnnouncementsTimer = null;
                }

                // stop speech recognition service
                await StopSpeechRecognition();

                await DisconnectAsync(WorkoutDuration);
            }
        }

        WorkoutModel CompletedWorkout;
        int workoutId = 0;
        private bool isDisconnecting;
        private async Task DisconnectAsync(TimeSpan workoutDuration)
        {

            if (isDisconnecting)
            {
                return;
            }


            if (!_isConnected)
            {
                return;
            }

            if (_isDetectingSpeech)
            {
                await StopSpeechRecognition();
            }

            isDisconnecting = true;
            if (_hrCharacteristic != null)
            {

                // try to disconnect
                try
                {
                    _hrCharacteristic.ValueUpdated -= _hrCharacteristic_ValueUpdated;
                    //await _hrCharacteristic.StopUpdatesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Stop reading characteristic error: {ex.Message}");
                }

                // reset values
                _hrService = null;
                _hrCharacteristic = null;
            }

            // Disconnect devices as session data no longer needed
            if (_connectedDevice != null && _adapter.ConnectedDevices.Count != 0)
            {
                try
                {
                    await _adapter.DisconnectDeviceAsync(_connectedDevice);
                    _connectedDevice?.Dispose();
                    _connectedDevice = null;
                    _isConnected = false;
                    Console.WriteLine("Disconnected device");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Disconnection Error: {ex.Message}");
                }
                finally
                {
                    isDisconnecting = false;
                }
            }

            // save data in SQLite db
            if (_numberOfHeartRateReadings != 0 && _heartRateList.Count > 0)
            {
                var HeartRateValsJson = Newtonsoft.Json.JsonConvert.SerializeObject(_heartRateList);
                var CompletedWorkout = new WorkoutModel()
                {
                    WorkoutStartTime = _workoutStartTime,
                    WorkoutDurationInTicks = workoutDuration.Ticks,
                    HeartRateValues = HeartRateValsJson,
                    SessionMaxHr = _sessionHeartRateMax,
                    SessionMinHr = _sessionHeartRateMin,
                    SessionAvgHr = _sessionHeartRateAvg
                };

                WorkoutDatabase database = await WorkoutDatabase.Instance;
                var result = await database.AddWorkoutAsync(CompletedWorkout);
                Console.WriteLine($"Number of new record(s) added: {result}");

                workoutId = CompletedWorkout.Id;

                try
                {
                    await Navigation.PushAsync(new WorkoutFinished(workoutId));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Navigation error: {ex.Message}");
                }
            }
        }

        // https://docs.microsoft.com/en-gb/xamarin/xamarin-forms/data-cloud/azure-cognitive-services/speech-recognition
        async void OnDemandData()
        {
            bool isMicEnabled = await micService.GetPermissionAsync();

            // EARLY OUT: make sure mic is accessible
            if (!isMicEnabled)
            {
                Console.WriteLine("Please grant access to the microphone!");
                return;
            }

            // initialize speech recognizer 
            if (recognizer == null)
            {
                // create list of known phrases to improve recognition accuracy 
                var config = SpeechConfig.FromSubscription(SpeechServiceConstants.CognitiveServicesApiKey, SpeechServiceConstants.CognitiveServicesRegion);
                recognizer = new SpeechRecognizer(config);

                var ExpectedPhraseList = PhraseListGrammar.FromRecognizer(recognizer);
                ExpectedPhraseList.AddPhrase("Heart rate");
                ExpectedPhraseList.AddPhrase("Workout duration");
                ExpectedPhraseList.AddPhrase("Maximum");
                ExpectedPhraseList.AddPhrase("Minimum");
                ExpectedPhraseList.AddPhrase("Min");
                ExpectedPhraseList.AddPhrase("Max");
                ExpectedPhraseList.AddPhrase("Session");
                ExpectedPhraseList.AddPhrase("Stop");
                ExpectedPhraseList.AddPhrase("Speech service");
                ExpectedPhraseList.AddPhrase("average");
                ExpectedPhraseList.AddPhrase("Workout duration");
                ExpectedPhraseList.AddPhrase("Current");
                ExpectedPhraseList.AddPhrase("Time");
                ExpectedPhraseList.AddPhrase("Callouts");
                ExpectedPhraseList.AddPhrase("Zone");

                //subscribe to Recognized event to get speech recognition results
                recognizer.Recognized += (obj, args) =>
                {
                    ExecuteDetectedVoiceCommands(args.Result.Text);
                };
            }

            Console.WriteLine($"Is detecting speech: {_isDetectingSpeech}");

            // if already trying to recognise speech, stop speech recognizer
            if (_isDetectingSpeech)
            {
                await StopSpeechRecognition();
            }
            else
            {
                await StartSpeechRecognition();
            }
        }

        private async Task StartSpeechRecognition()
        {
            Console.WriteLine("Starting speech recognition");

            try
            {
                audioService?.ActivateAudioRecordingSession();

                // continuous speech recognition will ensure STT engine is listening for the duration of the workout
                // in contrast, single-shot recognition only recognizes a single utterance.
                // The end of a single utterance is determined by listening for silence at the end,
                // or until a maximum of 15 seconds of audio is processed.
                await recognizer.StartContinuousRecognitionAsync();

                //todo: look into StartKeywordRecognitionAsync()
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Starting speech recognition error: {ex.Message}");
            }
            finally
            {
                _isDetectingSpeech = true;
                UpdateDisplayState();
            }
        }

        bool HasDisabledAudioAnnouncements; 
        async void ExecuteDetectedVoiceCommands(string newText)
        {
            if (!string.IsNullOrWhiteSpace(newText))
            {
                Console.WriteLine($"{newText}\n");

                // get current heart rate
                bool HrRequested = newText.ToLower()
                                          .Contains("current heart rate");
                if (HrRequested)
                {
                    var HrCallout = " Current heart rate: " + HeartRateLbl + "bpm.";

                    CallOutMetrics(HrCallout);
                }

                // get min heart rate
                bool AvgHrRequested = newText.ToLower()
                                          .Contains("average heart rate");
                if (AvgHrRequested)
                {
                    var AvgHrCallout = "Average heart rate: " + _sessionHeartRateAvg.ToString() + "bpm. ";

                    CallOutMetrics(AvgHrCallout);
                }

                // get current time
                bool CurrentTimeRequested = newText.ToLower()
                                          .Contains("current time");

                if (CurrentTimeRequested)
                {
                    var CurrentTimeCallout = "Current time: " + DateTime.Now.ToString("HH:mm") + ". ";

                    CallOutMetrics(CurrentTimeCallout);
                }

                // get workout duration
                bool WorkoutDurationRequested = newText.ToLower()
                          .Contains("workout duration");

                if (WorkoutDurationRequested)
                {
                    string currentDuration = GetCurrentWorkoutDuration();

                    CallOutMetrics(currentDuration);
                }

                // stop callouts
                bool StopCalloutsRequested = newText.ToLower()
                                                    .Contains("stop announcements");
                if (StopCalloutsRequested && HasDisabledAudioAnnouncements == false)
                {
                    Console.WriteLine("Stopping audio announcements");
                    HasDisabledAudioAnnouncements = true;
                    //CallOutMetrics("Stopping announcements");
                    StopAudioAnnouncements();
                }

                // resume callouts
                bool IsResumeCalloutsRequested = newText.ToLower()
                                                    .Contains("resume announcements");
                if (IsResumeCalloutsRequested && HasDisabledAudioAnnouncements == true)
                {
                    Console.WriteLine("Resuming announcements");
                    //CallOutMetrics("resuming announcements");
                    HasDisabledAudioAnnouncements = false;
                }

                // stop speech recognition
                bool CancelSpeechRecognitionRequested = newText.ToLower()
                                    .Contains("stop speech service");
                if (CancelSpeechRecognitionRequested)
                {
                    if (_isDetectingSpeech)
                    {
                        IsSpeechRecognitionCancelled = true;
                        await StopSpeechRecognition();
                    }
                }

                // get hr zone
                bool IsGetHrZoneRequested = newText.ToLower()
                                    .Contains("current zone");
                if (IsGetHrZoneRequested)
                {
                    // find out which hr zone user is currently in
                    var currentHrZone = HrZoneGetter.GetHrZone(Convert.ToInt32(HeartRateLbl));

                    string ZoneCallout = "";

                    if(currentHrZone == 0)
                    {
                        ZoneCallout = "Current heart rate zone is below zone 1. ";
                    }
                    else if (currentHrZone == 6)
                    {
                        ZoneCallout = "Current heart rate zone is above maximum heart rate. ";
                    }
                    else
                    {
                        ZoneCallout = "Current heart rate zone: " + currentHrZone.ToString() + ". ";
                    }

                    CallOutMetrics(ZoneCallout);

                }
            }
        }

        void UpdateDisplayState()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (_isDetectingSpeech)
                {
                    OnDemandDataIndicator.IsRunning = true;
                }
                else
                {
                    OnDemandDataIndicator.IsRunning = false;
                }
            });
        }

        private async Task StopSpeechRecognition()
        {

            Console.WriteLine("Stopping speech recognition");

            try
            {
                await recognizer?.StopContinuousRecognitionAsync();
                audioService?.ActivateAudioPlaybackSession();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Stopping speech recognition error: {ex.Message}");
            }
            finally
            {
                _isDetectingSpeech = false;
                UpdateDisplayState();
            }
        }

        public void StopAudioAnnouncements()
        {
            // if cancellation not requested (i.e. false) then return
            if (_cts?.IsCancellationRequested ?? true) //https://stackoverflow.com/questions/37851873/what-does-mean-after-variable-in-c/37852031
            {
                return;
            }
            else
            {
                Console.WriteLine("Audio announcements cancelled");
                try
                {
                    _cts.Cancel();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("cancelling audio announcements: ", ex.Message);
                }
                
            }
        }

        

        // todo: Add more to the workout page: heart rate zones
        // todo: ability to turn on and off audio announcements during workout?
    }
}