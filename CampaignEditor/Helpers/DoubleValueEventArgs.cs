using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor.Helpers
{
    public class DoubleValueEventArgs
    {
        public double Value { private set; get; } = 0.0f;

        public DoubleValueEventArgs(double value)
        {
            Value = value;
        }
    }
}
