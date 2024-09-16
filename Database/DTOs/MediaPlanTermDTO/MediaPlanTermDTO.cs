using System;

namespace Database.DTOs.MediaPlanTermDTO
{
    public class MediaPlanTermDTO : BaseIdentityMediaPlanTermDTO
    {
        public MediaPlanTermDTO(int xmptermid, int xmpid, DateOnly date, string? spotcode, string? added, string? deleted) 
            : base(xmptermid, xmpid, date, spotcode, added, deleted)
        {
        }

    }
}
