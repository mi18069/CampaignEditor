using System;

namespace Database.DTOs.MediaPlanTermDTO
{
    public class BaseMediaPlanTermDTO
    {
        public BaseMediaPlanTermDTO(int xmpid, DateOnly date, string? spotcode, string? added, string? deleted)
        {
            this.xmpid = xmpid;
            this.date = date;
            this.spotcode = spotcode;
            this.added = added;
            this.deleted = deleted;
        }

        public int xmpid { get; set; }
        public DateOnly date { get; set; }
        public string? spotcode { get; set; }
        public string? added { get; set; }
        public string? deleted { get; set; }
    }
}
