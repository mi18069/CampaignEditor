using System;

namespace Database.Entities
{
    public class MediaPlan
    {
        public int xmpid { get; set; }
        public int schid { get; set; }
        public int cmpid { get; set; }
        public int chid { get; set; }
        public string name { get; set; }
        public int version { get; set; }
        public string position { get; set; }
        public string stime { get; set; }
        public string etime { get; set; }
        public string blocktime { get; set; }
        public string days { get; set; }
        public string type { get; set; }
        public bool special { get; set; }
        public DateOnly sdate { get; set; }
        public DateOnly edate { get; set; }
        public float progcoef { get; set; }
        public DateOnly created { get; set; }
        public DateOnly modified { get; set; }
        public double amr1 { get; set; }
        public double amr2 { get; set; }
        public double amr3 { get; set; }
        public double amrsale { get; set; }
        public double amrp1 { get; set; }
        public double amrp2 { get; set; }
        public double amrp3 { get; set; }
        public double amrpsale { get; set; }
        public double dpcoef { get; set; }
        public double seascoef { get; set; }
        public double price { get; set; }
        public bool active { get; set; }

    }
}
