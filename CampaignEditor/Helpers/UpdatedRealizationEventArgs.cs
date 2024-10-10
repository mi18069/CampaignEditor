using Database.DTOs.SpotDTO;
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
        public SpotDTO? Spot { get; private set; }
        public int Chrdsid { get; private set; }

        public UpdatedRealizationEventArgs(DateOnly date, SpotDTO? spot, int chrdsid)
        {
            Date = date;
            Spot = spot;
            Chrdsid = chrdsid;
        }
    }
}
