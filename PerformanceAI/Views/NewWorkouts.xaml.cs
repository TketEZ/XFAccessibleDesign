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

        private ObservableCollection<IDevice> Sensors = new ObservableCollection<IDevice>();
        private ObservableCollection<IDevice> ConnectedSensors = new ObservableCollection<IDevice>();


        private IDevice _device;
        private CancellationTokenSource _cancelTokenSource;
        private CancellationToken _cancelToken;

        private IService _hrService;
        private ICharacteristic _hrCharacteristic;

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

        private bool _isConnectingToChosenSensor;
        public bool IsConnectingToChosenSensor
        {
            get
            {
                return _isConnectingToChosenSensor;
            }
            set
            {
                _isConnectingToChosenSensor = value;
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
            SensorsListView.ItemsSource = Sensors;
            ConnectedSensorsListView.ItemsSource = ConnectedSensors;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _adapter.DeviceDiscovered += _adapter_DeviceDiscovered;
            _adapter.ScanTimeoutElapsed += _adapter_ScanTimeoutElapsed;
            _adapter.DeviceConnectionLost += _adapter_DeviceConnectionLost;
            _ble.StateChanged += _ble_StateChanged;

            HandleScanning();

        }

        private async void HandleScanning()
        {
            if (!IsScanning)
            {
                await StartScanning();
            }
            else
            {
                await StopScanning();
            }
        }

        private void _ble_StateChanged(object sender, Plugin.BLE.Abstractions.EventArgs.BluetoothStateChangedArgs e)
        {
            Console.WriteLine($"The bluetooth state changed to {e.NewState}");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _adapter.DeviceDiscovered -= _adapter_DeviceDiscovered;
            _adapter.ScanTimeoutElapsed -= _adapter_ScanTimeoutElapsed;
            _adapter.DeviceConnectionLost -= _adapter_DeviceConnectionLost;
            _ble.StateChanged -= _ble_StateChanged;
        }

        private void _adapter_DeviceConnectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            if (Sensors.Count == 0)
            {
                DisplayAlert("Connection Error", "Scan timed out because no devices were found. Please try scanning again and make sure your device is turned on and ready.", "Ok");
                Console.WriteLine("No device found");
            }

            IsScanning = false;
            //UpdateUI();

        }

        private void _adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(e.Device.Name))
                {
                    Sensors.Add(e.Device);
                }
                Console.WriteLine($"Device found {e.Device.Name}, {e.Device.Id}");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        private async Task StartScanning()
        {
            _device = null;
            _hrService = null;
            _hrCharacteristic = null;

            Console.WriteLine("Starting scan");
            IsScanning = true;
            //UpdateUI();

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

            //UpdateUI();
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

            //UpdateUI();

        }

        private async void SensorsListView_ItemSelected(System.Object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {

            var _chosenSensor = e.SelectedItem as IDevice;

            if (_chosenSensor == null)
            {
                return;
            }

            // remove this when trying to add multiple sensors so scan continues  
            await StopScanning();

            IsConnectingToChosenSensor = true;
            // todo: add TTS to say "Connecting to sensor"
            await ConnectToChosenSensor(_chosenSensor); // connect to device
            await GetHrDataFromChosenSensor(_chosenSensor); // get hr data
            IsConnectingToChosenSensor = false;

            if (_chosenSensor.State == DeviceState.Connected)
            {
                // add to connected sensors list 
                if (!string.IsNullOrWhiteSpace(_chosenSensor.Name))
                {
                    ConnectedSensors.Add(_chosenSensor);
                }
                Console.WriteLine($"Device connected {_chosenSensor.Name}");
            }

            // clear selection
            ((ListView)sender).SelectedItem = null;
            Sensors.Remove(_chosenSensor);
        }

        private async Task GetHrDataFromChosenSensor(IDevice _chosenSensor)
        {
            // get Heart rate Service
            try
            {
                _hrService = await _chosenSensor.GetServiceAsync(HeartRateIdentifiers.HeartRateService);
                Console.WriteLine($"Heart Rate Service: {_hrService}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting hr Service data {ex.Message}");
            }

            // get Heart rate Characteristic
            try
            {
                _hrCharacteristic = await _hrService.GetCharacteristicAsync(HeartRateIdentifiers.HeartRateCharacteristic);
                Console.WriteLine($"Heart Rate Characteristics: {_hrCharacteristic}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting hr characteristic data {ex.Message}");
            }

            // todo: what to do with this? 
            //if (_hrCharacteristic != null)
            //{
            //    _hrCharacteristic.ValueUpdated += _hrCharacteristic_ValueUpdated;
            //    await _hrCharacteristic.StartUpdatesAsync();
            //}
        }

        private async Task ConnectToChosenSensor(IDevice _chosenSensor)
        {
            try
            {
                await _adapter.ConnectToDeviceAsync(_chosenSensor);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Device Connection Error", "Error connecting to device. Please try again.", "Ok");
                Console.WriteLine($"Cannot read data from device {ex.Message}");
            }
        }

        async void NextButton_Clicked(System.Object sender, System.EventArgs e)
        {
            //await Navigation.PushAsync(new SpeechSettingsConfiguration(_device, _adapter));
        }

        //private void UpdateUI()
        //{
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        if (IsScanning == true)
        //        {
        //            ScanButton.Text = "Stop Scanning";
        //            Sensors.Clear();
        //        }
        //        else
        //        {
        //            ScanButton.Text = "Start scanning for devices";
        //        }
        //    });
        //}
    }
}
