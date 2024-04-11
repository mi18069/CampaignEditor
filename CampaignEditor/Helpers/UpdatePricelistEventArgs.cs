using Database.DTOs.PricelistDTO;
using System;

namespace CampaignEditor.Helpers
{
    public class UpdatePricelistEventArgs : EventArgs
    {
        public PricelistDTO pricelist { get; private set; }

        public UpdatePricelistEventArgs(PricelistDTO pricelist)
        {
            this.pricelist = pricelist;
        }
    }
}
