using Database.DTOs.RealizedSpotDTO;
using Database.DTOs.SpotDTO;
using System.Collections.Generic;

namespace CampaignEditor.UserControls.ValidationItems.PairSpotsItems
{
    public class EqualSpots
    {
        public SpotDTO ExpectedSpot { get; set; }
        public List<RealizedSpotDTO> RealizedSpots { get; set; }

        public EqualSpots(SpotDTO expectedSpot, List<RealizedSpotDTO> realizedSpots)
        {
            ExpectedSpot = expectedSpot;
            RealizedSpots = new List<RealizedSpotDTO>(realizedSpots);
        }
    }
}
