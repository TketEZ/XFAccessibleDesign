using System;
namespace PerformanceAI.Helpers
{
    public class HeartRateIdentifiers
    {
        // primary service for heart rate 
        public static Guid HeartRateService = Guid.ParseExact("0000180d-0000-1000-8000-00805f9b34fb", "d");

        // characteristics within heart rate service
        public static Guid HeartRateCharacteristic = Guid.ParseExact("00002a37-0000-1000-8000-00805f9b34fb", "d");
        public static Guid BodySensorLocationCharacteristic = Guid.ParseExact("00002A38-0000-1000-8000-00805f9b34fb", "d");
        private static Guid ClientCharacteristicConfig = Guid.ParseExact("00002902-0000-1000-8000-00805f9b34fb", "d");

        // primary service for battery level 
        public static Guid BatteryService = Guid.ParseExact("0000180f-0000-1000-8000-00805f9b34fb", "d");


        // characteristic within battery servicei (read and notify)
        public static Guid BatteryLevelCharacteristics = Guid.ParseExact("00002a19-0000-1000-8000-00805f9b34fb", "d");
    }
}
