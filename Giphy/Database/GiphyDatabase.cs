using System;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Giphy.Database
{
    public class GiphyDatabase
    {

        public GiphyDatabase()
        {
            
        }

        public static void CreateDatabase()
        {
            try
            {
                var conn = new SQLiteConnection(Global.databaseFile);

                conn.CreateTable<Favorites>();
                conn.CreateTable<Recents>();

                conn.Close();
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
            }
        }

        public IEnumerable<Favorites> GetFavorites(SQLiteConnection conn)
        {
            return (from i in conn.Table<Favorites>() select i);
        }

        public Favorites GetFavorite(SQLiteConnection conn, string giphy_id)
        {
            return conn.Table<Favorites>().Where(i => i.Giphy_Id == giphy_id).FirstOrDefault();
        }

        public static void insertUpdateFavorite(Favorites data)
        {
            try
            {
                var conn = new SQLiteConnection(Global.databaseFile);

                if (conn.Insert(data) != 0)
                    conn.Update(data);

                conn.Close();
            }
            catch (SQLiteException e)
            {
                Debug.WriteLine("DB EXCEPTION: " + e.Message);
            }
        }
    }
}
