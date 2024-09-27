using System;

namespace Database.DTOs.MediaPlanDTO
{
    public class BaseIdentityMediaPlanDTO : BaseMediaPlanDTO
    {
        public BaseIdentityMediaPlanDTO(int xmpid, int schid, int cmpid, int chid, string name, int version, string position, string stime, string? etime, string? blocktime, string days, string type, bool special, DateOnly sdate, DateOnly? edate, decimal progcoef, DateOnly created, DateOnly? modified, decimal amr1, int amr1trim, decimal amr2, int amr2trim, decimal amr3, int amr3trim, decimal amrsale, int amrsaletrim, decimal amrp1, decimal amrp2, decimal amrp3, decimal amrpsale, decimal dpcoef, decimal seascoef, decimal seccoef, decimal coefA, decimal coefB, decimal cbrcoef, decimal price, bool active, decimal pps)
            : base(schid, cmpid, chid, name, version, position, stime, etime, blocktime, days, type, special, sdate, edate, progcoef, created, modified, amr1, amr1trim, amr2, amr2trim, amr3, amr3trim, amrsale, amrsaletrim, amrp1, amrp2, amrp3, amrpsale, dpcoef, seascoef, seccoef, coefA, coefB, cbrcoef, price, active, pps)
        {
            this.xmpid = xmpid;
        }

        public BaseIdentityMediaPlanDTO(MediaPlanDTO mediaPlan)
            : base(mediaPlan.schid, mediaPlan.cmpid, mediaPlan.chid, mediaPlan.name, mediaPlan.version, mediaPlan.position, mediaPlan.stime, mediaPlan.etime, mediaPlan.blocktime, mediaPlan.days, mediaPlan.type, mediaPlan.special, mediaPlan.sdate, mediaPlan.edate, mediaPlan.progcoef, mediaPlan.created, mediaPlan.modified, mediaPlan.amr1, mediaPlan.amr1trim, mediaPlan.amr2, mediaPlan.amr2trim, mediaPlan.amr3, mediaPlan.amr3trim, mediaPlan.amrsale, mediaPlan.amrsaletrim, mediaPlan.amrp1, mediaPlan.amrp2, mediaPlan.amrp3, mediaPlan.amrpsale, mediaPlan.dpcoef, mediaPlan.seascoef, mediaPlan.seccoef, mediaPlan.coefA, mediaPlan.coefB, mediaPlan.cbrcoef, mediaPlan.price, mediaPlan.active, mediaPlan.pps)
        {
            this.xmpid = mediaPlan.xmpid;
        }
        public int xmpid { get; set; }

    }
}
