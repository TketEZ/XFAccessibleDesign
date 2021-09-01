using System;
using System.Collections.Generic;
//using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PerformanceAI.Models;
using PerformanceAI.Services;
using SQLite;
using Xamarin.Forms;

[assembly: Dependency(typeof(WorkoutDatabase))]
namespace PerformanceAI.Services
{
    public class WorkoutDatabase
    {
        // https://docs.microsoft.com/en-us/xamarin/get-started/tutorials/local-database/
        // https://github.com/jamesmontemagno/app-mycadence/blob/features-2021/MyCadence/Helpers/RideDatabase.cs

        static SQLiteAsyncConnection Database;

        public static readonly AsyncLazy<WorkoutDatabase> Instance = new AsyncLazy<WorkoutDatabase>(async () =>
        {
            var instance = new WorkoutDatabase();
            CreateTableResult result = await Database.CreateTableAsync<WorkoutModel>();
            CreateTableResult TrainingZoneTableResult = await Database.CreateTableAsync<TrainingZoneModel>();
            return instance;
        });

        public WorkoutDatabase()
        {
            Database = new SQLiteAsyncConnection(SQLiteDbConstants.DatabasePath, SQLiteDbConstants.Flags);
        }

        #region DB Function related to Workouts

        public async Task<IEnumerable<WorkoutModel>> GetAllWorkoutsAsync()
        {
            var OldWorkouts = await Database.QueryAsync<WorkoutModel>("select * from WorkoutModel order by DateUtc DESC");
            return OldWorkouts;
        }

        public async Task<WorkoutModel> GetWorkoutAsync(int id)
        {
            var workout = await Database.Table<WorkoutModel>()
                .Where(i => i.Id == id).FirstOrDefaultAsync();

            return workout;
        }

        public async Task<int> AddWorkoutAsync(WorkoutModel workout)
        {
            Console.WriteLine($"Adding workout that completed on {workout.ShortDateListView} at {workout.ShortTimeListView}");

            if (workout.Id != 0)
            {
                // update existing workout
                var result = await Database.UpdateAsync(workout);
                return result;

            }
            else
            {
                //save a new workout
                var result = await Database.InsertAsync(workout);
                return result;
            }
        }

        public async Task RemoveWorkoutAsync(int id)
        {
            var workout = await Database.DeleteAsync<WorkoutModel>(id);
        }

        #endregion

        #region DB functions related to training zones

        public async Task<int> AddTrainingZoneAsync(TrainingZoneModel TrainingZone)
        {
            if (TrainingZone.Id != 0)
            {
                // update existing workout
                var result = await Database.UpdateAsync(TrainingZone);
                return result;

            }
            else
            {
                //save a new workout
                var result = await Database.InsertAsync(TrainingZone);
                return result;
            }
        }

        public async Task<IEnumerable<TrainingZoneModel>> GetAllTrainingZonesAsync()
        {
            var TrainingZones = await Database.Table<TrainingZoneModel>().ToListAsync();
            return TrainingZones;
        }

        public async Task<TrainingZoneModel> GetTrainingZoneAsync(int id)
        {
            var TrainingZone = await Database.Table<TrainingZoneModel>()
                .Where(i => i.Id == id).FirstOrDefaultAsync();

            return TrainingZone;
        }

        public async Task DeleteTrainingZoneAsync(int id)
        {
            var result = await Database.DeleteAsync<TrainingZoneModel>(id);
        }

        #endregion DB functions related to training zones

    }
}
