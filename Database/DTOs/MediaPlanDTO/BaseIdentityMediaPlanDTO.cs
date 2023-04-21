using System;

namespace Database.DTOs.MediaPlanDTO
{
    public class BaseIdentityMediaPlanDTO : BaseMediaPlanDTO
    {
        public BaseIdentityMediaPlanDTO(int xmpid, int schid, int cmpid, int chid, string name, int version, string position, string stime, string? etime, string? blocktime, string days, string type, bool special, DateOnly sdate, DateOnly? edate, float progcoef, DateOnly created, DateOnly? modified, double amr1, double amr2, double amr3, double amrsale, double amrp1, double amrp2, double amrp3, double amrpsale, double dpcoef, double seascoef, double price, bool active)
            : base(schid, cmpid, chid, name, version, position, stime, etime, blocktime, days, type, special, sdate, edate, progcoef, created, modified, amr1, amr2, amr3, amrsale, amrp1, amrp2, amrp3, amrpsale, dpcoef, seascoef, price, active)
        {
            this.xmpid = xmpid;
        }

        public BaseIdentityMediaPlanDTO(MediaPlanDTO mediaPlan)
            : base(mediaPlan.schid, mediaPlan.cmpid, mediaPlan.chid, mediaPlan.name, mediaPlan.version, mediaPlan.position, mediaPlan.stime, mediaPlan.etime, mediaPlan.blocktime, mediaPlan.days, mediaPlan.type, mediaPlan.special, mediaPlan.sdate, mediaPlan.edate, mediaPlan.progcoef, mediaPlan.created, mediaPlan.modified, mediaPlan.amr1, mediaPlan.amr2, mediaPlan.amr3, mediaPlan.amrsale, mediaPlan.amrp1, mediaPlan.amrp2, mediaPlan.amrp3, mediaPlan.amrpsale, mediaPlan.dpcoef, mediaPlan.seascoef, mediaPlan.price, mediaPlan.active)
        {
            this.xmpid = mediaPlan.xmpid;
        }
        public int xmpid { get; set; }

    }
}
