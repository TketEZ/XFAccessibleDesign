using System;
using System.IO;

namespace PerformanceAI.Services
{
    public static class SQLiteDbConstants
    {

        // https://github.com/xamarin/xamarin-forms-samples/blob/main/Todo/Todo/Todo/Constants.cs

        public const string DatabaseFilename = "PerformanceAiSQLite.db3";

        public const SQLite.SQLiteOpenFlags Flags =
            // open the database in read/write mode
            SQLite.SQLiteOpenFlags.ReadWrite |
            // create the database if it doesn't exist
            SQLite.SQLiteOpenFlags.Create;

        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }

    }
}
