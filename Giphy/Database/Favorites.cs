using System;
using SQLite;

namespace Giphy.Database
{
    public class Favorites
    {
        public Favorites() { }
        [PrimaryKey]
        public int Id { get; set; }
        [Unique, NotNull]
        public string Giphy_Id { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
