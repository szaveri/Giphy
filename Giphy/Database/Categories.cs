using System;
using SQLite;

namespace Gifology.Database
{
    public class Categories
    {
        public Categories() { }
        [PrimaryKey]
        public int? Id { get; set; } = null;
        [Unique, NotNull]
        public string Name { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
