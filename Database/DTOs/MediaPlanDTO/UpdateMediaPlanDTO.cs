using System;

namespace Database.DTOs.MediaPlanDTO
{
    public class UpdateMediaPlanDTO : BaseIdentityMediaPlanDTO
    {
        public UpdateMediaPlanDTO(int xmpid, int schid, int cmpid, int chid, string name, int version, string position, string stime, string? etime, string? blocktime, string days, string type, bool special, DateOnly sdate, DateOnly? edate, decimal progcoef, DateOnly created, DateOnly? modified, decimal amr1, int amr1trim, decimal amr2, int amr2trim, decimal amr3, int amr3trim, decimal amrsale, int amrsaletrim, decimal amrp1, decimal amrp2, decimal amrp3, decimal amrpsale, decimal dpcoef, decimal seascoef, decimal seccoef, decimal coefA, decimal coefB, decimal price, bool active, decimal pps)
            : base(xmpid, schid, cmpid, chid, name, version, position, stime, etime, blocktime, days, type, special, sdate, edate, progcoef, created, modified, amr1, amr1trim, amr2, amr2trim, amr3, amr3trim, amrsale, amrsaletrim, amrp1, amrp2, amrp3, amrpsale, dpcoef, seascoef, seccoef, coefA, coefB, price,  active, pps)
        {
        }

        public UpdateMediaPlanDTO(MediaPlanDTO mediaPlan)
        : base(mediaPlan)
        {
        }
    }
}
