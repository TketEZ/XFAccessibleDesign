using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PerformanceAI.Helpers;
using PerformanceAI.Models;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PerformanceAI.Views
{
    public partial class NewWorkouts : ContentPage
    {

        readonly IAdapter _adapter;
        readonly IBluetoothLE _ble;

        private ObservableCollection<IDevice> devices = new ObservableCollection<IDevice>();
        private IDevice _device;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken;

        private IService _hrService;
        private ICharacteristic _hrCharacteristic;
        private IService _batteryService;
        private ICharacteristic _batteryCharacteristic;

        private WorkoutModel _currentWorkoutValues;

        private bool _isScanning;
        public bool IsScanning
        {
            get
            {
                return _isScanning;
            }
            set
            {
                _isScanning = value;
                OnPropertyChanged();
            }
        }

        private string _heartRate = "Heart Rate";
        public string HeartRate
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

        private string _startTime = "Start Time";
        public string StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                _startTime = value;
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

        public NewWorkouts()
        {
            InitializeComponent();
            BindingContext = this;

            _ble = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;
            _adapter.ScanTimeout = 20000;
            DevicesListView.ItemsSource = devices;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _adapter.DeviceDiscovered += _adapter_DeviceDiscovered;
            _adapter.ScanTimeoutElapsed += _adapter_ScanTimeoutElapsed;
            _adapter.DeviceConnectionLost += _adapter_DeviceConnectionLost;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _adapter.DeviceDiscovered -= _adapter_DeviceDiscovered;
            _adapter.ScanTimeoutElapsed -= _adapter_ScanTimeoutElapsed;
            _adapter.DeviceConnectionLost -= _adapter_DeviceConnectionLost;
        }

        private void _adapter_DeviceConnectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            if (devices.Count == 0)
            {
                DisplayAlert("Connection Error", "Scan timed out because no devices were found. Please try scanning again and make sure your device is turned on and ready.", "Ok");
                Console.WriteLine("No device found");
            }

            IsScanning = false;
            UpdateUI();

        }

        private void _adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(e.Device.Name))
                {
                    devices.Add(e.Device);
                }
                Console.WriteLine($"Device found {e.Device.Name}, {e.Device.Id}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        private void StatusButton_Clicked(System.Object sender, System.EventArgs e)
        {
            var state = _ble.State;

            _ble.StateChanged += (s, ex) =>
            {
                Console.WriteLine($"The bluetooth state changed to {ex.NewState}");
                Debug.WriteLine($"Hey Bluetooth state is {ex.NewState}");
            };

            DisplayAlert("Bluetooth Status: ", state.ToString(), "Ok");
            Console.WriteLine($"Bluetooth status: {state}");
        }

        private async void ScanButton_Clicked(System.Object sender, System.EventArgs e)
        {
            Console.WriteLine("Scan Button Clicked");

            if (!IsScanning)
            {
                await StartScanning();
            }
            else
            {
                await StopScanning();
            }
        }

        private async Task StartScanning()
        {
            _device = null;
            _hrService = null;
            _hrCharacteristic = null;
            _batteryService = null;
            _batteryCharacteristic = null;

            IsScanning = true;
            UpdateUI();

            try
            {
                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    var status = await DependencyService.Get<ILocationPermission>().CheckAndRequestLocationPermission();

                    if (status != PermissionStatus.Granted)
                    {
                        return;
                    }
                }

                if (!_ble.IsAvailable || _ble.State != BluetoothState.On)
                {
                    if (await DisplayAlert("Bluetooth Status Error", "Bluetooth is not available or is turned off. Please check bluetooth settings.", "Open Settings", "OK"))
                    {
                        AppInfo.ShowSettingsUI();
                    }
                    return;
                }

                Guid[] HrServiceGuid = new[] { HeartRateIdentifiers.HeartRateService };
                _cancelTokenSource = new CancellationTokenSource();
                _cancelToken = _cancelTokenSource.Token;
                await _adapter.StartScanningForDevicesAsync(HrServiceGuid, cancellationToken: _cancelToken);

            }
            catch (Exception ex)
            {
                await DisplayAlert("Bluetooth Scanning Error", "Scan failed, please try again.", "OK");
                Console.WriteLine($"Scan failed {ex.Message}");
                return;
            }
            finally
            {
                IsScanning = false;
            }

            UpdateUI();
        }

        private async Task StopScanning()
        {

            // if not scanning, don't need to stop
            if (!IsScanning)
            {
                return;
            }

            // cancel scan
            if (_cancelTokenSource != null)
            {
                _cancelTokenSource?.Cancel();
                _cancelTokenSource?.Dispose();
                _cancelTokenSource = null;
            }

            // stop adapter from scanning
            try
            {
                await _adapter.StopScanningForDevicesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with stopping scan: {ex.Message}");
            }
            // check this
            finally
            {
                IsScanning = false;
            }

            UpdateUI();

        }

        private async void DevicesListView_ItemSelected(System.Object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            
            var _device = e.SelectedItem as IDevice;

            if (_device == null)
            {
                return;
            }

            await StopScanning();

            try
            {

                await _adapter.ConnectToDeviceAsync(_device);

            }
            catch (Exception ex)
            {
                await DisplayAlert("Device Connection Error", $"Cannot read data from device {ex.Message}", "Ok");
                Console.WriteLine("Error connecting to device");
            }

            //todo: move connect to characteristics stuff here

            await Navigation.PushAsync(new SpeechSettingsConfiguration(_device, _adapter));

            if (((ListView)sender).SelectedItem == null)
            {
                return;
            }

            ((ListView)sender).SelectedItem = null; // clear selection
            devices.Clear();
        }

        private void UpdateUI()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (IsScanning == true)
                {
                    ScanButton.Text = "Stop Scanning";
                    devices.Clear();
                }
                else
                {
                    ScanButton.Text = "Start scanning for devices";
                }
            });
        }
    }
}
