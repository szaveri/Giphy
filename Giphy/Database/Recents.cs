﻿using System;
using SQLite;

namespace Gifology.Database
{
    public class Recents
    {
        public Recents() { }
        [PrimaryKey]
        public int? Id { get; set; } = null;
        [Unique, NotNull]
        public string Giphy_Id { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
