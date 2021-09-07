using System;
using PerformanceAI.Services;
using PerformanceAI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SQLite;
using SQLitePCL;
using System.IO;

namespace PerformanceAI
{
    public partial class App : Application
    {
        // https://channel9.msdn.com/Shows/XamarinShow/Improving-Accessibility-with-Xamarin-Community-Toolkit
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new Homepage());
        }

        protected override async void OnStart()
        {
            //var Database = new SQLiteAsyncConnection(SQLiteDbConstants.DatabasePath, SQLiteDbConstants.Flags);
            //await Database.ExecuteAsync("DROP TABLE Zone");
            //await Database.ExecuteAsync("DROP TABLE WorkoutModel");
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
