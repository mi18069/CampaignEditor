using System;

namespace Database.DTOs.MediaPlanTermDTO
{
    public class BaseMediaPlanTermDTO
    {
        public BaseMediaPlanTermDTO(int xmpid, DateOnly date, string spotcode)
        {
            this.xmpid = xmpid;
            this.date = date;
            this.spotcode = spotcode;
        }

        public int xmpid { get; set; }
        public DateOnly date { get; set; }
        public string spotcode { get; set; }
    }
}
