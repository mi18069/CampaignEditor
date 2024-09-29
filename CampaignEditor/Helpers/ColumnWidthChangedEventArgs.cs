using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor.Helpers
{
    public class ColumnWidthChangedEventArgs
    {
        public double Width { get; private set; }
        public int Index { get; private set; }

        public ColumnWidthChangedEventArgs(double width, int index)
        {
            Width = width;
            Index = index;
        }
    }
}
