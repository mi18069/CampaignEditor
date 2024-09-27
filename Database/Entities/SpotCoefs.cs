using System.Collections.Generic;

namespace Database.Entities
{
    public class SpotCoefs
    {

        public List<DateRangeSeasCoef> dateRanges { get; private set; }
        public Dictionary<int, decimal> cbrCoefs { get; private set; } 
        public decimal seccoef { get; set; }

        public SpotCoefs(List<DateRangeSeasCoef> dateRanges, Dictionary<int, decimal> cbrCoefs, decimal seccoef)
        {
            this.dateRanges = dateRanges;
            this.cbrCoefs = cbrCoefs;
            this.seccoef = seccoef;
        }

    }
}
