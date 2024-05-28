using Database.DTOs.ChannelDTO;
using Database.DTOs.SpotDTO;

namespace Database.Entities
{
    public class TermTuple
    {
        private MediaPlan _mediaPlan;
        private MediaPlanTerm _mediaPlanTerm;
        private SpotDTO _spot;
        private decimal _price;
        private int _status = 1;
        public string ChannelName { get; set; }

        public int Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public MediaPlan MediaPlan { get { return _mediaPlan; } }
        public MediaPlanTerm MediaPlanTerm { get { return _mediaPlanTerm; } }
        public SpotDTO Spot { get { return _spot; } }
        private TermCoefs _termCoefs;
        public decimal Price { get { return _termCoefs.Price; } }
        public decimal Seccoef { get { return _termCoefs.Seccoef; } }
        public decimal Seascoef { get { return _termCoefs.Seascoef; } }
        public decimal? Cpp { get { return _termCoefs.Cpp; } }
        public decimal? Amrpsale { get { return _termCoefs.Amrpsale; } }

        public TermTuple(MediaPlan mediaPlan, MediaPlanTerm mediaPlanTerm, 
            SpotDTO spot, TermCoefs termCoefs, string channelname)
        {
            _mediaPlan = mediaPlan;
            _mediaPlanTerm = mediaPlanTerm;
            _spot = spot;
            _termCoefs = termCoefs;
            this.ChannelName = channelname;
            if (mediaPlan.xmpid == null || mediaPlan.xmpid == 0)
            {
                this.Status = -1;
            }
        }
    }
}
