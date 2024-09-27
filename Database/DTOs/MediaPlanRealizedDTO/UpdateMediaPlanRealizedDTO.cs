using Database.Entities;

namespace Database.DTOs.MediaPlanRealizedDTO
{
    public class UpdateMediaPlanRealizedDTO : BaseIdentityMediaPlanRealizedDTO
    {
        public UpdateMediaPlanRealizedDTO(int id, int cmpid, string name, int stime, int etime, string stimestr, string etimestr, int chid, int dure, int durf, string date, int emsnum, int posinbr, int totalspotnum, int breaktype, int spotnum, int brandnum, decimal amrp1, decimal amrp2, decimal amrp3, decimal amrpsale, decimal cpp, decimal dpcoef, decimal seascoef, decimal seccoef, decimal progcoef, decimal price, int status, decimal chcoef, decimal coefA, decimal coefB, decimal cbrcoef, bool accept) 
            : base(id, cmpid, name, stime, etime, stimestr, etimestr, chid, dure, durf, date, emsnum, posinbr, totalspotnum, breaktype, spotnum, brandnum, amrp1, amrp2, amrp3, amrpsale, cpp, dpcoef, seascoef, seccoef, progcoef, price, status, chcoef, coefA, coefB, cbrcoef, accept)
        {
        }
        public UpdateMediaPlanRealizedDTO(MediaPlanRealizedDTO mpRealizedDTO)
            : base(mpRealizedDTO.id, mpRealizedDTO.cmpid, mpRealizedDTO.name, mpRealizedDTO.stime, mpRealizedDTO.etime, mpRealizedDTO.stimestr, mpRealizedDTO.etimestr, mpRealizedDTO.chid, mpRealizedDTO.dure, mpRealizedDTO.durf, mpRealizedDTO.date, mpRealizedDTO.emsnum, mpRealizedDTO.posinbr, mpRealizedDTO.totalspotnum, mpRealizedDTO.breaktype, mpRealizedDTO.spotnum, mpRealizedDTO.brandnum, mpRealizedDTO.amrp1, mpRealizedDTO.amrp2, mpRealizedDTO.amrp3, mpRealizedDTO.amrpsale, mpRealizedDTO.cpp, mpRealizedDTO.dpcoef, mpRealizedDTO.seascoef, mpRealizedDTO.seccoef, mpRealizedDTO.progcoef, mpRealizedDTO.price, mpRealizedDTO.status, mpRealizedDTO.chcoef, mpRealizedDTO.coefA, mpRealizedDTO.coefB, mpRealizedDTO.cbrcoef, mpRealizedDTO.accept)
        {
        }

    }
}
