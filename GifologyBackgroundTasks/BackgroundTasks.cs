using SQLite;
using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace GifologyBackgroundTasks
{
    public sealed class UpdateTask : IBackgroundTask
    {
        string SettingsPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "settings.db");
        ApplicationDataContainer RoamingSettings = ApplicationData.Current.RoamingSettings;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            if(IsEarlierVersion("1.5.12"))
            {
                using (SQLiteConnection conn = new SQLiteConnection(SettingsPath))
                {
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

            //Update latest version
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            string LatestVersion = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            RoamingSettings.Values["CurrentVersion"] = LatestVersion;
        }

        private bool IsEarlierVersion(string Version)
        {
            if (RoamingSettings.Values["CurrentVersion"] == null)
            {
                return true;
            }
            else
            {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;

                var CurrentVersion = (string)RoamingSettings.Values["CurrentVersion"];
                var CurrentSplit = CurrentVersion.Split('.');

                var CompareSplit = Version.Split('.');

                if (Convert.ToInt32(CurrentSplit[0]) <= Convert.ToInt32(CompareSplit[0]))
                    return true;
                if (Convert.ToInt32(CurrentSplit[1]) <= Convert.ToInt32(CompareSplit[1]))
                    return true;
                if (Convert.ToInt32(CurrentSplit[2]) <= Convert.ToInt32(CompareSplit[2]))
                    return true;
            }

            return false;
        }
    }
}
