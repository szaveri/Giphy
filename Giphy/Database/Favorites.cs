using System;
using SQLite;

namespace Gifology.Database
{
    public class Favorites
    {
        public Favorites() { }
        [PrimaryKey]
        public int? Id { get; set; } = null;
        [NotNull]
        public string Giphy_Id { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int Category { get; set; } = 1;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
