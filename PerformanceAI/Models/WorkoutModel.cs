using System;
using SQLite;

namespace PerformanceAI.Models
{
    public class WorkoutModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string HeartRateValues { get; set; }

        public DateTime WorkoutStartTime { get; set; }

        public long WorkoutDurationInTicks { get; set; }

        public int SessionMaxHr { get; set; }

        public int SessionMinHr { get; set; }

        public int SessionAvgHr { get; set; }

        // todo: allow user to give workout a name (and possibly description?)
        //public string WorkoutName;

        //https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/null-coalescing-operator
        //https://stackoverflow.com/questions/37851873/what-does-mean-after-variable-in-c/37852031
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/member-access-operators#null-conditional-operators--and-
        // requires c# v8


        // Old Workouts page items:
        public DateTime DateUtc { get; set; } = DateTime.UtcNow;

        DateTime? localDate;
        [Ignore]
        DateTime LocalDate => localDate ??= DateUtc.ToLocalTime();

        string shortDisplayDate;
        [Ignore]
        public string ShortDateListView => shortDisplayDate ??= $"{LocalDate:ddd, MMM d}";

        string shortDisplayTime;
        [Ignore]
        public string ShortTimeListView => shortDisplayTime ??= $"{LocalDate.ToShortTimeString()}";

        string longDisplayDate;
        [Ignore]
        public string LongDate => longDisplayDate ??= $"{LocalDate.ToLongDateString()}";

        string longDisplayTime;
        [Ignore]
        public string LongTime => longDisplayTime ??= $"{LocalDate.ToLongTimeString()}";

    }
}
