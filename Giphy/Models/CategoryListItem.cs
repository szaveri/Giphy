using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gifology
{
    public class CategoryListItem
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public bool? IsChecked { get; set; } = false;
    }
}
