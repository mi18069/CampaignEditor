using System;

namespace Database.DTOs.MediaPlanHistDTO
{
    public class BaseMediaPlanHistDTO
    {
        public BaseMediaPlanHistDTO(int xmpid, int schid, int chid, string name, string position, string stime, string? etime, DateOnly date, float progcoef, decimal amr1, decimal amr2, decimal amr3, decimal amrsale, decimal amrp1, decimal amrp2, decimal amrp3, decimal amrpsale, bool active, bool outlier)
        {
            this.xmpid = xmpid;
            this.schid = schid;
            this.chid = chid;
            this.name = name;
            this.position = position;
            this.stime = stime;
            this.etime = etime;
            this.date = date;
            this.progcoef = progcoef;
            this.amr1 = amr1;
            this.amr2 = amr2;
            this.amr3 = amr3;
            this.amrsale = amrsale;
            this.amrp1 = amrp1;
            this.amrp2 = amrp2;
            this.amrp3 = amrp3;
            this.amrpsale = amrpsale;
            this.active = active;
            this.outlier = outlier;
        }

        public int xmpid { get; set; }
        public int schid { get; set; }
        public int chid { get; set; }
        public string name { get; set; }
        public string position { get; set; }
        public string stime { get; set; }
        public string? etime { get; set; }
        public DateOnly date { get; set; }
        public float progcoef { get; set; }
        public decimal amr1 { get; set; }
        public decimal amr2 { get; set; }
        public decimal amr3 { get; set; }
        public decimal amrsale { get; set; }
        public decimal amrp1 { get; set; }
        public decimal amrp2 { get; set; }
        public decimal amrp3 { get; set; }
        public decimal amrpsale { get; set; }
        public bool active { get; set; }
        public bool? outlier { get; set; }
    }
}
