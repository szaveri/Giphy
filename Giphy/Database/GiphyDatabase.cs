﻿using System;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Giphy.Database
{
    public class GiphyDatabase
    {
        private static SQLiteAsyncConnection aconn = new SQLiteAsyncConnection(Global.databaseFile);

        public GiphyDatabase()
        {

        }

        public static void CreateDatabase()
        {
            try
            {
                var conn = new SQLiteConnection(Global.databaseFile);

                if (!IfTableExists(conn, "Favorites"))
                    conn.CreateTable<Favorites>();
                if (!IfTableExists(conn, "Recents"))
                    conn.CreateTable<Recents>();

                conn.Close();
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
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
            var list = new List<Favorites>();
            var query = conn.Table<Favorites>();

            foreach (var fav in query)
                list.Add(fav);

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
            var list = new List<Recents>();
            var query = conn.Table<Recents>();

            foreach (var recent in query)
                list.Add(recent);

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
