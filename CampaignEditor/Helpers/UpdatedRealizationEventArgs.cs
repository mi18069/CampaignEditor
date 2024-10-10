using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor.Helpers
{
    public class UpdatedRealizationEventArgs : EventArgs
    {
        public DateOnly Date { get; private set; }
        public char Spotcode { get; private set; }
        public int Chrdsid { get; private set; }

        public UpdatedRealizationEventArgs(DateOnly date, char spotcode, int chrdsid)
        {
            Date = date;
            Spotcode = spotcode;
            Chrdsid = chrdsid;
        }
    }
}
