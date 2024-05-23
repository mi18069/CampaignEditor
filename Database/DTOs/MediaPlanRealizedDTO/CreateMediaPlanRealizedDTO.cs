
namespace Database.DTOs.MediaPlanRealizedDTO
{
    public class CreateMediaPlanRealizedDTO : BaseMediaPlanRealizedDTO
    {
        public CreateMediaPlanRealizedDTO(int cmpid, string name, int stime, int etime, string stimestr, string etimestr, int chid, int dure, int durf, string date, int emsnum, int posinbr, int totalspotnum, int breaktype, int spotnum, int brandnum, decimal amrp1, decimal amrp2, decimal amrp3, decimal amrpsale, decimal cpp, decimal dpcoef, decimal seascoef, decimal seccoef, decimal progcoef, decimal price, int status) 
            : base(cmpid, name, stime, etime, stimestr, etimestr, chid, dure, durf, date, emsnum, posinbr, totalspotnum, breaktype, spotnum, brandnum, amrp1, amrp2, amrp3, amrpsale, cpp, dpcoef, seascoef, seccoef, progcoef, price, status)
        {
        }
    }
}
