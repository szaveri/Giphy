using System;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Gifology.Database
{
    public class GifologyDatabase
    {
        private static SQLiteAsyncConnection aconn = new SQLiteAsyncConnection(Global.databaseFile);
        private static SQLiteAsyncConnection saconn = new SQLiteAsyncConnection(Global.settingsFile);

        public GifologyDatabase()
        {

        }

        public static void CreateDatabase()
        {
            using (var conn = new SQLiteConnection(Global.databaseFile))
            {
                try
                {
                    conn.BeginTransaction();

                    if (!IfTableExists(conn, "Categories"))
                        conn.CreateTable<Categories>();

                    if (conn.Table<Categories>().Where(x => x.Name == "Uncategorized").Count() == 0)
                    {
                        var uncategorized = new Categories();
                        uncategorized.Id = 1;
                        uncategorized.Name = "Uncategorized";
                        conn.Insert(uncategorized);
                    }


                    if (!IfTableExists(conn, "Favorites"))
                        conn.CreateTable<Favorites>();
                    if (!IfTableExists(conn, "Recents"))
                        conn.CreateTable<Recents>();

                    conn.Commit();
                }
                catch (SQLiteException e)
                {
                    Debug.WriteLine("DB EXCEPTION: " + e.Message);
                    conn.Rollback();
                }
            }

        }

        public static void CreateSettingsDatabase()
        {
            using (var conn = new SQLiteConnection(Global.settingsFile))
            {
                try
                {
                    conn.BeginTransaction();

                    if (!IfTableExists(conn, "Settings"))
                        conn.CreateTable<Settings>();

                    if (conn.Table<Settings>().Where(x => x.Id == 1).Count() == 0)
                    {
                        var setting = new Settings();
                        setting.Id = 1;
                        setting.InfiniteScroll = 0;
                        setting.GifQuality = "Medium";
                        setting.StartPage = "Search";
                        conn.Insert(setting);
                    }

                    conn.Commit();
                }
                catch (SQLiteException e)
                {
                    Debug.WriteLine("DB EXCEPTION: " + e.Message);
                    conn.Rollback();
                }
            }

        }

        public static bool IfTableExists(SQLiteConnection conn, string table)
        {
            var tableQuery = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=?;";
            bool tableExists = conn.ExecuteScalar<int>(tableQuery, table) == 1;

            return tableExists;
        }

        /* ==================================
         * START FAVORITE DATABASE FUNCTIONS
         * ==================================
         */
        public static async Task<List<Favorites>> GetFavorites(int limit)
        {
            return await aconn.QueryAsync<Favorites>("SELECT * FROM Favorites GROUP BY Giphy_Id ORDER BY Timestamp DESC LIMIT ?,?", limit, limit + Global.limit + 1);
        }

        public static Favorites GetFavorite(string giphy_id)
        {
            using (var conn = new SQLiteConnection(Global.databaseFile))
            {
                return conn.Table<Favorites>().Where(i => i.Giphy_Id == giphy_id).FirstOrDefault();

            }
        }

        public static async Task<List<Favorites>> GetFavoritesInCategory(int category_id, int limit)
        {
            return await aconn.Table<Favorites>().Where(i => i.Category == category_id)
                .OrderBy(x => x.Timestamp)
                .Skip(limit)
                .Take(Global.limit + 1)
                .ToListAsync();
        }

        public static async void InsertUpdateFavorite(Favorites data)
        {
            try
            {
                await aconn.InsertOrReplaceAsync(data);
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
            }
        }

        public static async void DeleteFavorite(Favorites data)
        {
            try
            {
                await aconn.DeleteAsync(data);
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
            }
        }

        public static async void DeleteFavoriteId(string giphy_id)
        {
            try
            {
                await aconn.QueryAsync<int>("DELETE FROM Favorites WHERE Giphy_Id = ?", giphy_id);
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
            }
        }

        /* ==================================
         * END FAVORITE DATABASE FUNCTIONS
         * ==================================
         */

        /* ==================================
         * START RECENT DATABASE FUNCTIONS
         * ==================================
         */
        public static async Task<List<Recents>> GetRecents(int limit)
        {
            //return await aconn.QueryAsync<Recents>("SELECT * FROM Recents ORDER BY Timestamp DESC LIMIT ?,?", limit, limit + Global.limit + 1);
            return await aconn.Table<Recents>()
               .OrderByDescending(x => x.Timestamp)
               .Skip(limit)
               .Take(Global.limit + 1)
               .ToListAsync();
        }

        public static async void InsertUpdateRecent(Recents data)
        {
            try
            {
                await aconn.InsertOrReplaceAsync(data);
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
            }
        }

        public static async void DeleteRecent(Recents data)
        {
            try
            {
                await aconn.DeleteAsync(data);
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
            }
        }

        /* ==================================
         * END RECENT DATABASE FUNCTIONS
         * ==================================
         */

        /* ==================================
         * START CATEGORY DATABASE FUNCTIONS
         * ==================================
         */

        public static async Task<List<Categories>> GetCategories()
        {
            return await aconn.Table<Categories>().OrderBy(x => x.Name).ToListAsync();
        }

        public static async Task<List<Categories>> GetCategoryList()
        {
            return await aconn.Table<Categories>().Where(i => i.Name != "Uncategorized").OrderBy(x => x.Name).ToListAsync();
        }

        public sealed class DistinctCategoryIdResult
        {
            public int Category_Id { get; set; }
        }

        public static async Task<int> GetCategoryId(string Name)
        {
            var result = await aconn.QueryAsync<DistinctCategoryIdResult>("SELECT Category FROM Categories WHERE Name = ?", Name);
            return result.Select(x => x.Category_Id).FirstOrDefault();
        }

        public sealed class DistinctCategoryResult
        {
            public int Category { get; set; }
        }

        public static async Task<List<int>> GetImageCategory(string giphy_id)
        {
            var result = await aconn.QueryAsync<DistinctCategoryResult>("SELECT Category FROM Favorites WHERE Giphy_Id = ? AND Category <> 1", giphy_id);
            return result.Select(x => x.Category).ToList();
        }

        public static async void DeleteImageInCategory(Categories data)
        {
            if (data.Id == 1)
                return;

            try
            {
                await aconn.ExecuteAsync("DELETE FROM Favorites WHERE Category = ?", data.Id);
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
            }
        }

        public static async void MoveImageToUncategorized(Categories data)
        {
            if (data.Id == 1)
                return;

            try
            {
                await aconn.ExecuteAsync("UPDATE Favorites SET Category = 1 WHERE Category = ?", data.Id);
                //Delete Duplicates
                await aconn.QueryAsync<Categories>("DELETE FROM Favorites WHERE Id IN (SELECT Id FROM Favorites GROUP BY Giphy_Id, Category HAVING COUNT(1) > 1)");
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
            }
        }

        public static async void InsertUpdateCategory(Categories data)
        {
            //Cannot update Uncategorized
            if (data.Id == 1)
                return;

            try
            {
                await aconn.InsertOrReplaceAsync(data);
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
            }
        }

        public static async void DeleteCategories(Categories data)
        {
            //Cannot delete Uncategorized
            if (data.Id == 1)
                return;

            try
            {
                await aconn.DeleteAsync(data);
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
            }
        }

        /* ==================================
         * END CATEGORY DATABASE FUNCTIONS
         * ==================================
         */

        /* ==================================
         * START SETTINGS DATABASE FUNCTIONS
         * ==================================
         */
        public static async void GetSettings()
        {
            try
            {
                await saconn.Table<Settings>().FirstOrDefaultAsync().ContinueWith(t =>
                {
                    var settings = t.Result;
                    SettingsItem.InfiniteScroll = settings.InfiniteScroll;
                    SettingsItem.AutoPlay = settings.AutoPlay;
                    SettingsItem.GifQuality = settings.GifQuality;
                    SettingsItem.StartPage = settings.StartPage;
                });
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
            }
        }

        public static async void InsertUpdateSettings()
        {
            try
            {
                var data = new Settings();
                data.Id = 1;
                data.InfiniteScroll = SettingsItem.InfiniteScroll;
                data.AutoPlay = SettingsItem.AutoPlay;
                data.GifQuality = SettingsItem.GifQuality;
                data.StartPage = SettingsItem.StartPage;
                await saconn.InsertOrReplaceAsync(data);
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
            }
        }
        /* ==================================
         * END SETTINGS DATABASE FUNCTIONS
         * ==================================
         */
    }
}
