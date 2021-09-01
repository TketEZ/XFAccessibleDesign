using System;
using PerformanceAI.Helpers;

namespace PerformanceAI.Utils
{
    public class HrZoneGetter
    {
        static int Zone1LowerBound = Settings.HrZone1LowerBound;
        static int Zone1UpperBound = Settings.HrZone1UpperBound;
        static int Zone2UpperBound = Settings.HrZone2UpperBound;
        static int Zone3UpperBound = Settings.HrZone3UpperBound;
        static int Zone4UpperBound = Settings.HrZone4UpperBound;
        static int MaxHr = Settings.MaxHr;


        public enum HrZone
        {
            BelowZone1,
            Zone1,
            Zone2,
            Zone3,
            Zone4,
            Zone5,
            AboveZone5
        }

        public static int GetHrZone(int CurrentHrValue)
        {

            bool IsHrBelowZone1 = CurrentHrValue < Zone1LowerBound;
            bool isHrInZone1 = Between(CurrentHrValue, Zone1LowerBound, Zone1UpperBound);
            bool IsHrInZone2 = Between(CurrentHrValue, Zone1UpperBound, Zone2UpperBound);
            bool IsHrInZone3 = Between(CurrentHrValue, Zone2UpperBound, Zone3UpperBound);
            bool IsHrInZone4 = Between(CurrentHrValue, Zone3UpperBound, Zone4UpperBound);
            bool IsHrInZone5 = Between(CurrentHrValue, Zone4UpperBound, MaxHr);
            bool IsHrAboveZone5 = CurrentHrValue > MaxHr;


            if (IsHrBelowZone1)
            {
                return (int)HrZone.BelowZone1;
            }
            else if (isHrInZone1)
            {
                return (int)HrZone.Zone1;
            }
            else if (IsHrInZone2)
            {
                return (int)HrZone.Zone2;
            }
            else if (IsHrInZone3)
            {
                return (int)HrZone.Zone3;
            }
            else if (IsHrInZone4)
            {
                return (int)HrZone.Zone4;
            }
            else if (IsHrInZone5)
            {
                return (int)HrZone.Zone5;
            }
            else if (IsHrAboveZone5)
            {
                return (int)HrZone.AboveZone5;
            }
            else
            {
                return -1;
            }
        }

        static bool Between(int CurrentHrVal, int RangeMin, int RangeMax)
        {
            return CurrentHrVal >= RangeMin && CurrentHrVal < RangeMax;
        }

    }
}
