using System;
using SQLite;

namespace PerformanceAI.Models
{
    [Table("Zone")]
    public class TrainingZoneModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Zone { get; set; }
        public double durationInMinutes { get; set; }

        //TimeSpan? durationAsMinutes;
        //[Ignore]
        //TimeSpan DurationAsMinutes = TimeSpan.FromTicks(durationAsTicks).TotalMinutes;

        //TimeSpan ts = TimeSpan.FromTicks(durationAsTicks);
        //double minutesFromTs = ts.TotalMinutes;

    }
}
