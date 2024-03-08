
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SpotDTO;

namespace Database.Entities
{
    public class TermTuple
    {
        private MediaPlanDTO _mediaPlan;
        private MediaPlanTermDTO _mediaPlanTerm;
        private SpotDTO _spot;

        public MediaPlanDTO MediaPlan { get { return _mediaPlan; } }
        public MediaPlanTermDTO MediaPlanTerm { get { return _mediaPlanTerm; } }
        public SpotDTO Spot { get { return _spot; } }

        public TermTuple(MediaPlanDTO mediaPlan, MediaPlanTermDTO mediaPlanTerm, SpotDTO spot)
        {
            _mediaPlan = mediaPlan;
            _mediaPlanTerm = mediaPlanTerm;
            _spot = spot;
        }
    }
}
