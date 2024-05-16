using Database.Entities;
using System;

namespace Database.DTOs.MediaPlanHistDTO
{
    public class CreateMediaPlanHistDTO : BaseMediaPlanHistDTO
    {
        public CreateMediaPlanHistDTO(int xmpid, int schid, int chid, string name, string position, string stime, string? etime, DateOnly date, float progcoef, decimal amr1, decimal amr2, decimal amr3, decimal amrsale, decimal amrp1, decimal amrp2, decimal amrp3, decimal amrpsale, bool active, bool outlier) 
            : base(xmpid, schid, chid, name, position, stime, etime, date, progcoef, amr1, amr2, amr3, amrsale, amrp1, amrp2, amrp3, amrpsale, active, outlier)
        {
        }

        public CreateMediaPlanHistDTO(MediaPlanHistDTO mpHistDTO)
        : base(mpHistDTO.xmpid, mpHistDTO.schid, mpHistDTO.chid, mpHistDTO.name, mpHistDTO.position, mpHistDTO.stime, mpHistDTO.etime, mpHistDTO.date, mpHistDTO.progcoef, mpHistDTO.amr1, mpHistDTO.amr2, mpHistDTO.amr3, mpHistDTO.amrsale, mpHistDTO.amrp1, mpHistDTO.amrp2, mpHistDTO.amrp3, mpHistDTO.amrpsale, mpHistDTO.active, mpHistDTO.outlier.Value)
        {
        }

        public CreateMediaPlanHistDTO(MediaPlanHist mpHist)
        : base(mpHist.xmpid, mpHist.schid, mpHist.chid, mpHist.name, mpHist.position, mpHist.stime, mpHist.etime, mpHist.date, mpHist.progcoef, mpHist.amr1, mpHist.amr2, mpHist.amr3, mpHist.amrsale, mpHist.amrp1, mpHist.amrp2, mpHist.amrp3, mpHist.amrpsale, mpHist.active, mpHist.outlier)
        {
        }
    }
}
