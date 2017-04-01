using System;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

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
            using(var conn = new SQLiteConnection(Global.databaseFile))
            {
                try
                {
                    conn.Execute("BEGIN TRANSACTION");

                    if (!IfTableExists(conn, "Favorites"))
                        conn.CreateTable<Favorites>();
                    if (!IfTableExists(conn, "Recents"))
                        conn.CreateTable<Recents>();
                    if (!IfTableExists(conn, "Categories"))
                    {
                        conn.CreateTable<Categories>();
                    }
                    if(conn.Table<Categories>().Where(x => x.Name == "Uncategorized").Count() == 0)
                    {
                        var uncategorized = new Categories();
                        uncategorized.Name = "Uncategorized";
                        conn.Insert(uncategorized);
                    }

                    conn.Execute("COMMIT TRANSACTION");
                }
                catch (SQLiteException e)
                {
                    Debug.WriteLine("DB EXCEPTION: " + e.Message);
                    conn.Execute("ROLLBACK");
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
        public static List<Favorites> GetFavorites(SQLiteConnection conn)
        {
            //return (from i in conn.Table<Favorites>() select i);
            var cmd = conn.CreateCommand("SELECT * FROM Favorites ORDER BY Timestamp DESC");
            var list = new List<Favorites>();
            list = cmd.ExecuteQuery<Favorites>();

            return list;
        }

        public static Favorites GetFavorite(SQLiteConnection conn, string giphy_id)
        {
            return conn.Table<Favorites>().Where(i => i.Giphy_Id == giphy_id).FirstOrDefault();
        }

        public static async void InsertUpdateFavorite(Favorites data)
        {
            try
            {
                if (data.Id == null)
                    await aconn.InsertAsync(data);
                else
                    await aconn.UpdateAsync(data);
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
        public static List<Recents> GetRecents(SQLiteConnection conn)
        {
            //return (from i in conn.Table<Favorites>() select i);
            var cmd = conn.CreateCommand("SELECT * FROM Recents ORDER BY Timestamp DESC");
            var list = new List<Recents>();
            list = cmd.ExecuteQuery<Recents>();

            return list;
        }

        public static async void InsertUpdateRecent(Recents data)
        {
            try
            {
                if (data.Id == null)
                    await aconn.InsertAsync(data);
                else
                    await aconn.UpdateAsync(data);
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
