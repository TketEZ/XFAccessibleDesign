using System;
using Xamarin.Essentials;

namespace PerformanceAI.Helpers
{
    public class Settings
    {

        // https://github.com/jamesmontemagno/app-mycadence/blob/main/MyCadence/Helpers/Settings.cs

        #region Device Info

        public static string SensorName
        {
            get
            {
                return Preferences.Get("SensorName", "Sensor Name");
            }
            set
            {
                Preferences.Set(SensorName, value);
            }
        }

        public static string SensorId
        {
            get
            {
                return Preferences.Get("SensorId", "Sensor ID");
            }
            set
            {
                Preferences.Set(SensorId, value);
            }
        }

        #endregion Device Info

        #region Interval Items

        public static bool IsIntervalAlertsEnabled
        {
            get
            {
                return Preferences.Get("IsIntervalAlertsEnabled", false);
            }
            set
            {
                Preferences.Set("IsIntervalAlertsEnabled", value);
            }
        }

        const int IntervalFrequencyDefault = 0;
        public static int IntervalFrequency
        {
            get
            {
                return Preferences.Get("IntervalFrequency", IntervalFrequencyDefault);
            }
            set
            {
                Preferences.Set("IntervalFrequency", value);
            }
        }

        public static bool IsCurrentHrAlertEnabled
        {
            get
            {
                return Preferences.Get("IsCurrentHrAlertEnabled", false);
            }
            set
            {
                Preferences.Set("IsCurrentHrAlertEnabled", value);
            }
        }

        public static bool isAvgHrEnabled
        {
            get
            {
                return Preferences.Get("isAvgHrEnabled", false);
            }
            set
            {
                Preferences.Set("isAvgHrEnabled", value);
            }
        }

        public static bool isMaxHrEnabled
        {
            get
            {
                return Preferences.Get("isMaxHrEnabled", false);
            }
            set
            {
                Preferences.Set("isMaxHrEnabled", value);
            }
        }

        public static bool isMinHrEnabled
        {
            get
            {
                return Preferences.Get("isMinHrEnabled", false);
            }
            set
            {
                Preferences.Set("isMinHrEnabled", value);
            }
        }

        public static bool isWorkoutDurationEnabled
        {
            get
            {
                return Preferences.Get("isWorkoutDurationEnabled", false);
            }
            set
            {
                Preferences.Set("isWorkoutDurationEnabled", value);
            }
        }

        public static bool isCurrentTimeEnabled
        {
            get
            {
                return Preferences.Get("isCurrentTimeEnabled", false);
            }
            set
            {
                Preferences.Set("isCurrentTimeEnabled", value);
            }
        }

        #endregion Interval Items

        #region Alarms

        public static bool IsHrZoneChangeAlertEnabled
        {
            get
            {
                return Preferences.Get("IsHrZoneChangeAlertEnabled", false);
            }
            set
            {
                Preferences.Set("IsHrZoneChangeAlertEnabled", value);
            }
        }

        public static bool isMaxHrAlarmEnabled
        {
            get
            {
                return Preferences.Get("isMaxHrAlarmEnabled", false);
            }
            set
            {
                Preferences.Set("isMaxHrAlarmEnabled", value);
            }
        }

        #endregion Alarms

        #region Audio Items

        const double VolumeValueDefault = 1;
        public static double VolumeValue
        {
            get
            {
                return Preferences.Get("VolumeValue", VolumeValueDefault);
            }
            set
            {
                Preferences.Set("VolumeValue", value);
            }
        }

        public static bool IsOnDemandDataenabled
        {
            get
            {
                return Preferences.Get("IsOnDemandDataenabled", false);
            }
            set
            {
                Preferences.Set("IsOnDemandDataenabled", value);
            }
        }

        #endregion Audio Items

        #region HrZoneSettings

        const int MaxHrDefault = 0;
        public static int MaxHr
        {
            get
            {
                return Preferences.Get("MaxHr", MaxHrDefault);
            }
            set
            {
                Preferences.Set("MaxHr", value);
            }
        }

        const int HrZone1LowerBoundDefault = 0;
        public static int HrZone1LowerBound
        {
            get
            {
                return Preferences.Get("HrZone1LowerBound", HrZone1LowerBoundDefault);
            }
            set
            {
                Preferences.Set("HrZone1LowerBound", value);
            }
        }

        const int HrZone1UpperBoundDefault = 0;
        public static int HrZone1UpperBound
        {
            get
            {
                return Preferences.Get("HrZone1UpperBound", HrZone1UpperBoundDefault);
            }
            set
            {
                Preferences.Set("HrZone1UpperBound", value);
            }
        }

        const int HrZone2UpperBoundDefault = 0;
        public static int HrZone2UpperBound
        {
            get
            {
                return Preferences.Get("HrZone2UpperBound", HrZone2UpperBoundDefault);
            }
            set
            {
                Preferences.Set("HrZone2UpperBound", value);
            }
        }

        const int HrZone3UpperBoundDefault = 0;
        public static int HrZone3UpperBound
        {
            get
            {
                return Preferences.Get("HrZone3UpperBound", HrZone3UpperBoundDefault);
            }
            set
            {
                Preferences.Set("HrZone3UpperBound", value);
            }
        }

        const int HrZone4UpperBoundDefault = 0;
        public static int HrZone4UpperBound
        {
            get
            {
                return Preferences.Get("HrZone4UpperBound", HrZone4UpperBoundDefault);
            }
            set
            {
                Preferences.Set("HrZone4UpperBound", value);
            }
        }

        const int HrZone5UpperBoundDefault = 0;
        public static int HrZone5UpperBound
        {
            get
            {
                return Preferences.Get("HrZone5UpperBound", HrZone5UpperBoundDefault);
            }
            set
            {
                Preferences.Set("HrZone5UpperBound", value);
            }
        }

        #endregion HrZoneSettings

        #region Workout Settings

        public static string ActivityType
        {
            get
            {
                return Preferences.Get("ActivityType", "Activity");
            }
            set
            {
                Preferences.Set(ActivityType, value);
            }
        }

        public static string TrainingIntent
        {
            get
            {
                return Preferences.Get("TrainingIntent", "Intent");
            }
            set
            {
                Preferences.Set(TrainingIntent, value);
            }
        }

        #endregion

    }
}
