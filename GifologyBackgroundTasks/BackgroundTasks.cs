using SQLite;
using System.IO;
using Windows.ApplicationModel.Background;

namespace GifologyBackgroundTasks
{
    public sealed class UpdateTask : IBackgroundTask
    {
        string SettingsPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "settings.db");

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            using(SQLiteConnection conn = new SQLiteConnection(SettingsPath)){
                try
                {
                    conn.BeginTransaction();

                    //Add Column
                    conn.Execute("ALTER TABLE `Settings` ADD COLUMN `AutoPlay` integer NOT NULL DEFAULT 1");
                    //Rename Table
                    conn.Execute("ALTER TABLE `Settings` RENAME TO `SettingsOld`");
                    //Create New Settings Table
                    conn.Execute("CREATE TABLE `Settings` (`Id`	integer NOT NULL,`InfiniteScroll` integer NOT NULL, `AutoPlay` integer NOT NULL DEFAULT 1, `GifQuality` varchar NOT NULL, `StartPage` varchar NOT NULL,`Timestamp`	bigint, PRIMARY KEY(`Id`))");
                    //Populate old date
                    conn.Execute("INSERT INTO Settings (Id, InfiniteScroll, AutoPlay, GifQuality, StartPage, Timestamp) SELECT Id, InfiniteScroll, AutoPlay, GifQuality, StartPage, Timestamp FROM SettingsOld");
                    //Drop Old Table
                    conn.Execute("DROP TABLE SettingsOld");

                    conn.Commit();
                }
                catch
                {
                    conn.Rollback();
                }
            }
        }
    }
}
