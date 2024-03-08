using System;

namespace Database.DTOs.MediaPlanHistDTO
{
    public class BaseIdentityMediaPlanHistDTO : BaseMediaPlanHistDTO
    {
        public BaseIdentityMediaPlanHistDTO(int xmphistid, int xmpid, int schid, int chid, string name, string position, string stime, string? etime, DateOnly date, float progcoef, double amr1, double amr2, double amr3, double amrsale, double amrp1, double amrp2, double amrp3, double amrpsale, bool active, bool outlier) 
            : base(xmpid, schid, chid, name, position, stime, etime, date, progcoef, amr1, amr2, amr3, amrsale, amrp1, amrp2, amrp3, amrpsale, active, outlier)
        {
            this.xmphistid = xmphistid;
        }

        public int xmphistid { get; set; }

    }
}
