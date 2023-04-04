using System;

namespace Database.DTOs.MediaPlanDTO
{
    public class BaseMediaPlanDTO
    {
        public BaseMediaPlanDTO(int schid, int cmpid, int chid, string name, int version, string position, string stime, string etime, string blocktime, string days, string type, bool special, DateOnly sdate, DateOnly edate, float progcoef, DateOnly created, DateOnly modified, double amr1, double amr2, double amr3, double amrsale, double amrp1, double amrp2, double amrp3, double amrpsale, double dpcoef, double seascoef, double price, bool active)
        {
            this.schid = schid;
            this.cmpid = cmpid;
            this.chid = chid;
            this.name = name;
            this.version = version;
            this.position = position;
            this.stime = stime;
            this.etime = etime;
            this.blocktime = blocktime;
            this.days = days;
            this.type = type;
            this.special = special;
            this.sdate = sdate;
            this.edate = edate;
            this.progcoef = progcoef;
            this.created = created;
            this.modified = modified;
            this.amr1 = amr1;
            this.amr2 = amr2;
            this.amr3 = amr3;
            this.amrsale = amrsale;
            this.amrp1 = amrp1;
            this.amrp2 = amrp2;
            this.amrp3 = amrp3;
            this.amrpsale = amrpsale;
            this.dpcoef = dpcoef;
            this.seascoef = seascoef;
            this.price = price;
            this.active = active;
        }

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
