using Database.DTOs.ChannelDTO;
using Database.DTOs.SpotDTO;

namespace Database.Entities
{
    public class TermTupleEmpty : TermTuple
    {
        public TermTupleEmpty(MediaPlan mediaPlan, MediaPlanTerm mediaPlanTerm, SpotDTO spot, TermCoefs termCoefs, string channelname) 
            : base(mediaPlan, mediaPlanTerm, spot, termCoefs, channelname)
        {
            this.Status = -1;
        }
    }
}
