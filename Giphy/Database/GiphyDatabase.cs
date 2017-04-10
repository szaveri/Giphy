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
            return await aconn.QueryAsync<Favorites>("SELECT * FROM Favorites ORDER BY Timestamp DESC LIMIT ?,?", limit, limit + Global.limit + 1);
        }

        public static Favorites GetFavorite(string giphy_id)
        {
            using (var conn = new SQLiteConnection(Global.databaseFile))
            {
                return conn.Table<Favorites>().Where(i => i.Giphy_Id == giphy_id).FirstOrDefault();

            }
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
            return await aconn.QueryAsync<Recents>("SELECT * FROM Recents ORDER BY Timestamp DESC LIMIT ?,?", limit, limit + Global.limit + 1);
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
    }
}
