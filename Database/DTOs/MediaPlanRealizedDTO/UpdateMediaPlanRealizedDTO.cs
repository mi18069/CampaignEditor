﻿namespace Database.DTOs.MediaPlanRealizedDTO
{
    public class UpdateMediaPlanRealizedDTO : BaseIdentityMediaPlanRealizedDTO
    {
        public UpdateMediaPlanRealizedDTO(int id, string name, int stime, int etime, int chid, int dure, int durf, string date, int emsnum, int posinbr, int totalspotnum, int breaktype, int spotnum, int brandnum, double amrp1, double amrp2, double amrp3, double amrpsale, double cpp, double dpcoef, double seascoef, double seccoef, double progcoef, double price, int status) : base(id, name, stime, etime, chid, dure, durf, date, emsnum, posinbr, totalspotnum, breaktype, spotnum, brandnum, amrp1, amrp2, amrp3, amrpsale, cpp, dpcoef, seascoef, seccoef, progcoef, price, status)
        {
        }
    }
}
