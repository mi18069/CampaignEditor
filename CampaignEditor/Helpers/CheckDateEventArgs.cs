using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor.Helpers
{
    public class CheckDateEventArgs
    {
        public DateOnly date { get; private set; }

        public CheckDateEventArgs(DateOnly date)
        {
            this.date = date;
        }
    }
}
