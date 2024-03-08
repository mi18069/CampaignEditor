using System;

namespace Database.DTOs.MediaPlanDTO
{
    public class UpdateMediaPlanDTO : BaseIdentityMediaPlanDTO
    {
        public UpdateMediaPlanDTO(int xmpid, int schid, int cmpid, int chid, string name, int version, string position, string stime, string? etime, string? blocktime, string days, string type, bool special, DateOnly sdate, DateOnly? edate, float progcoef, DateOnly created, DateOnly? modified, double amr1, int amr1trim, double amr2, int amr2trim, double amr3, int amr3trim, double amrsale, int amrsaletrim, double amrp1, double amrp2, double amrp3, double amrpsale, double dpcoef, double seascoef, double seccoef, double price, bool active, double pps)
            : base(xmpid, schid, cmpid, chid, name, version, position, stime, etime, blocktime, days, type, special, sdate, edate, progcoef, created, modified, amr1, amr1trim, amr2, amr2trim, amr3, amr3trim, amrsale, amrsaletrim, amrp1, amrp2, amrp3, amrpsale, dpcoef, seascoef, seccoef, price, active, pps)
        {
        }

        public UpdateMediaPlanDTO(MediaPlanDTO mediaPlan)
        : base(mediaPlan)
        {
        }
    }
}
