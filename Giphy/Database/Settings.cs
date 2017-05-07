using System;
using SQLite;

namespace Gifology.Database
{
    public class Settings
    {
        public Settings() { }
        [PrimaryKey]
        public int? Id { get; set; } = null;
        [Unique, NotNull]
        public int InfiniteScroll { get; set; } = 0;
        [Unique, NotNull]
        public string GifQuality { get; set; } = "Medium";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
