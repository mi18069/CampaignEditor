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

        public string StatusStr
        {
            get { return _status.ToString(); }
        }

        public MediaPlan MediaPlan { get { return _mediaPlan; } }
        public MediaPlanTerm MediaPlanTerm { get { return _mediaPlanTerm; } }
        public SpotDTO Spot { get { return _spot; } }
        private TermCoefs _termCoefs;
        public decimal? Price { get { return Status == -1 ? null : _termCoefs.Price; } }
        public decimal? Chcoef { get { return Status == -1 ? null : MediaPlan.Chcoef; } }
        public decimal? Seccoef { get { return Status == -1 ? null : _termCoefs.Seccoef; } }
        public decimal? Seascoef { get { return Status == -1 ? null : _termCoefs.Seascoef; } }
        public decimal? Progcoef { get { return Status == -1 ? null : MediaPlan.Progcoef; } }
        public decimal? CoefA { get { return Status == -1 ? null : MediaPlan.CoefA; } }
        public decimal? CoefB { get { return Status == -1 ? null : MediaPlan.CoefB; } }
        public decimal? Cpp { get { return Status == -1 ? null : _termCoefs.Cpp; } }
        public decimal? Amrpsale { get { return Status == -1 ? null : _termCoefs.Amrpsale; } }
        public int? Spotlength { get { return _spot.spotlength == 0 ? null : _spot.spotlength; } }

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
