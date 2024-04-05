using System;

namespace Database.Entities
{
    public class DateRangeSeasCoef
    {
        public DateOnly fromDate { get; set; }
        public DateOnly toDate { get; set; }
        public double seascoef { get; set; }


        public DateRangeSeasCoef(DateOnly fromDate, DateOnly toDate, double seascoef)
        {
            this.fromDate = fromDate;
            this.toDate = toDate;
            this.seascoef = seascoef;
        }

        public override string ToString() 
        { 
            return fromDate.ToShortDateString() + " - " + toDate.ToShortDateString();
        }
    }
}
