using Database.Entities;
using System;

namespace Database.DTOs.MediaPlanHistDTO
{
    public class UpdateMediaPlanHistDTO : BaseIdentityMediaPlanHistDTO
    {
        public UpdateMediaPlanHistDTO(int xmphistid, int xmpid, int schid, int chid, string name, string position, string stime, string? etime, DateOnly date, float progcoef, double amr1, double amr2, double amr3, double amrsale, double amrp1, double amrp2, double amrp3, double amrpsale, bool active, bool outlier) 
            : base(xmphistid, xmpid, schid, chid, name, position, stime, etime, date, progcoef, amr1, amr2, amr3, amrsale, amrp1, amrp2, amrp3, amrpsale, active, outlier)
        {
        }

        public UpdateMediaPlanHistDTO(MediaPlanHistDTO mediaPlanHistDTO)
            : base(mediaPlanHistDTO.xmphistid, mediaPlanHistDTO.xmpid, mediaPlanHistDTO.schid, mediaPlanHistDTO.chid, mediaPlanHistDTO.name, mediaPlanHistDTO.position, mediaPlanHistDTO.stime, mediaPlanHistDTO.etime, mediaPlanHistDTO.date, mediaPlanHistDTO.progcoef, mediaPlanHistDTO.amr1, mediaPlanHistDTO.amr2, mediaPlanHistDTO.amr3, mediaPlanHistDTO.amrsale, mediaPlanHistDTO.amrp1, mediaPlanHistDTO.amrp2, mediaPlanHistDTO.amrp3, mediaPlanHistDTO.amrpsale, mediaPlanHistDTO.active, mediaPlanHistDTO.outlier.Value)
        {
        }

        public UpdateMediaPlanHistDTO(MediaPlanHist mediaPlanHist)
            : base(mediaPlanHist.xmphistid, mediaPlanHist.xmpid, mediaPlanHist.schid, mediaPlanHist.chid, mediaPlanHist.name, mediaPlanHist.position, mediaPlanHist.stime, mediaPlanHist.etime, mediaPlanHist.date, mediaPlanHist.progcoef, mediaPlanHist.amr1, mediaPlanHist.amr2, mediaPlanHist.amr3, mediaPlanHist.amrsale, mediaPlanHist.amrp1, mediaPlanHist.amrp2, mediaPlanHist.amrp3, mediaPlanHist.amrpsale, mediaPlanHist.active, mediaPlanHist.outlier)
        {
        }
    }
}
