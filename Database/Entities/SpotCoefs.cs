using System.Collections.Generic;

namespace Database.Entities
{
    public class SpotCoefs
    {

        public List<DateRangeSeasCoef> dateRanges { get; private set; }
        public double seccoef { get; set; }

        public SpotCoefs(List<DateRangeSeasCoef> dateRanges, double seccoef)
        {
            this.dateRanges = dateRanges;
            this.seccoef = seccoef;
        }

    }
}
