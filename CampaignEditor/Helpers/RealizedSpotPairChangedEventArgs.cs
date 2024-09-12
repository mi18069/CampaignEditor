using Database.DTOs.RealizedSpotDTO;

namespace CampaignEditor.Helpers
{
    public class RealizedSpotPairChangedEventArgs
    {
        public RealizedSpotDTO RealizedSpot {get; private set;}
        public string Spotcode { get; private set; }

        public RealizedSpotPairChangedEventArgs(RealizedSpotDTO realizedSpot, string spotcode)
        {
            RealizedSpot = realizedSpot;
            Spotcode = spotcode;
        }
    }
}
