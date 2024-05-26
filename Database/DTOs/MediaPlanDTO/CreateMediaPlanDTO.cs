using Database.Entities;
using System;

namespace Database.DTOs.MediaPlanDTO
{
    public class CreateMediaPlanDTO : BaseMediaPlanDTO
    {
        public CreateMediaPlanDTO(int schid, int cmpid, int chid, string name, int version, string position, string stime, string? etime, string? blocktime, string days, string type, bool special, DateOnly sdate, DateOnly? edate, decimal progcoef, DateOnly created, DateOnly? modified, decimal amr1, int amr1trim, decimal amr2, int amr2trim, decimal amr3, int amr3trim, decimal amrsale, int amrsaletrim, decimal amrp1, decimal amrp2, decimal amrp3, decimal amrpsale, decimal dpcoef, decimal seascoef, decimal seccoef, decimal coefA, decimal coefB, decimal price, bool active, decimal pps)
            : base(schid, cmpid, chid, name, version, position, stime, etime, blocktime, days, type, special, sdate, edate, progcoef, created, modified, amr1, amr1trim, amr2, amr2trim, amr3, amr3trim, amrsale, amrsaletrim, amrp1, amrp2, amrp3, amrpsale, dpcoef, seascoef, seccoef, coefA, coefB, price, active, pps)
        {
        }

        public CreateMediaPlanDTO(MediaPlanDTO mediaPlanDTO)
            : base(mediaPlanDTO.schid, mediaPlanDTO.cmpid, mediaPlanDTO.chid, mediaPlanDTO.name, mediaPlanDTO.version, mediaPlanDTO.position, mediaPlanDTO.stime, mediaPlanDTO.etime, mediaPlanDTO.blocktime, mediaPlanDTO.days, mediaPlanDTO.type, mediaPlanDTO.special, mediaPlanDTO.sdate, mediaPlanDTO.edate, mediaPlanDTO.progcoef, mediaPlanDTO.created, mediaPlanDTO.modified,
                  mediaPlanDTO.amr1, mediaPlanDTO.amr1trim, mediaPlanDTO.amr2, mediaPlanDTO.amr2trim, mediaPlanDTO.amr3, mediaPlanDTO.amr3trim, mediaPlanDTO.amrsale, mediaPlanDTO.amrsaletrim, mediaPlanDTO.amrp1, mediaPlanDTO.amrp2, mediaPlanDTO.amrp3, mediaPlanDTO.amrpsale, mediaPlanDTO.dpcoef, mediaPlanDTO.seascoef, mediaPlanDTO.seccoef, mediaPlanDTO.coefA, mediaPlanDTO.coefB, mediaPlanDTO.price, mediaPlanDTO.active, mediaPlanDTO.pps)

        {
        }

        public CreateMediaPlanDTO(MediaPlan mediaPlan)
            : base(mediaPlan.schid, mediaPlan.cmpid, mediaPlan.chid, mediaPlan.name, mediaPlan.version, mediaPlan.position, mediaPlan.stime, mediaPlan.etime, mediaPlan.blocktime, mediaPlan.days, mediaPlan.type, mediaPlan.special, mediaPlan.sdate, mediaPlan.edate, mediaPlan.Progcoef, mediaPlan.created, mediaPlan.modified,
                  mediaPlan.Amr1, mediaPlan.Amr1trim, mediaPlan.Amr2, mediaPlan.Amr2trim, mediaPlan.Amr3, mediaPlan.Amr3trim, mediaPlan.Amrsale, mediaPlan.Amrsaletrim, mediaPlan.Amrp1, mediaPlan.Amrp2, mediaPlan.Amrp3, mediaPlan.Amrpsale, mediaPlan.Dpcoef, mediaPlan.Seascoef, mediaPlan.Seccoef, mediaPlan.coefA, mediaPlan.coefB, mediaPlan.Price, mediaPlan.active, mediaPlan.pps)
        {
        }
    }
}
