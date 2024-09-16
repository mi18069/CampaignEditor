using System;

namespace Database.DTOs.MediaPlanTermDTO
{
    public class BaseIdentityMediaPlanTermDTO : BaseMediaPlanTermDTO
    {
        public BaseIdentityMediaPlanTermDTO(int xmptermid, int xmpid, DateOnly date, string? spotcode, string? added, string? deleted) 
            : base(xmpid, date, spotcode, added, deleted)
        {
            this.xmptermid = xmptermid;
        }
        public int xmptermid { get; set; }

    }
}
