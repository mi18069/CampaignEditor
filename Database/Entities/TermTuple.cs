
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.DTOs.SpotDTO;

namespace Database.Entities
{
    public class TermTuple
    {
        private MediaPlan _mediaPlan;
        private MediaPlanTerm _mediaPlanTerm;
        private SpotDTO _spot;
        private decimal _price;

        public MediaPlan MediaPlan { get { return _mediaPlan; } }
        public MediaPlanTerm MediaPlanTerm { get { return _mediaPlanTerm; } }
        public SpotDTO Spot { get { return _spot; } }

        public decimal Price { get { return _price; } }

        public TermTuple(MediaPlan mediaPlan, MediaPlanTerm mediaPlanTerm, SpotDTO spot,
            decimal price)
        {
            _mediaPlan = mediaPlan;
            _mediaPlanTerm = mediaPlanTerm;
            _spot = spot;
            _price = price;
        }
    }
}
