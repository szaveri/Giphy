using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gifology
{
    public class Paging
    {
        public int Offset { get; set; } = 0;
        public int PreviousOffset { get; set; } = 0;
        public int Total { get; set; } = int.MaxValue;
        public bool PreviousEnabled = false;
        public bool NextEnabled = false;

        public ObservableCollection<GiphyImage> ColumnOneList = new ObservableCollection<GiphyImage>();
        public ObservableCollection<GiphyImage> ColumnTwoList = new ObservableCollection<GiphyImage>();
    }
}
