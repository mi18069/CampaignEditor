using System;

namespace Database.Entities
{
    public class MediaPlanHist
    {
        public int xmphistid { get; set; }
        public int xmpid { get; set; }
        public int schid { get; set; }
        public int chid { get; set; }
        public string name { get; set; }
        public string position { get; set; }
        public string stime { get; set; }
        public string? etime { get; set; }
        public DateOnly date { get; set; }
        public float progcoef { get; set; }
        public double amr1 { get; set; }
        public double amr2 { get; set; }
        public double amr3 { get; set; }
        public double amrsale { get; set; }
        public double amrp1 { get; set; }
        public double amrp2 { get; set; }
        public double amrp3 { get; set; }
        public double amrpsale { get; set; }
        public bool active { get; set; }
        public bool outlier { get; set; }
    }
}
