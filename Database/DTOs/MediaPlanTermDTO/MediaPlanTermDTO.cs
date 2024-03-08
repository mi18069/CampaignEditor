using System;

namespace Database.DTOs.MediaPlanTermDTO
{
    public class MediaPlanTermDTO : BaseIdentityMediaPlanTermDTO
    {
        public MediaPlanTermDTO(int xmptermid, int xmpid, DateOnly date, string? spotcode) 
            : base(xmptermid, xmpid, date, spotcode)
        {
        }

    }
}
