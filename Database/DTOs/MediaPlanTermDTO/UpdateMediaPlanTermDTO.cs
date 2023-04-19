using System;

namespace Database.DTOs.MediaPlanTermDTO
{
    public class UpdateMediaPlanTermDTO : BaseIdentityMediaPlanTermDTO
    {
        public UpdateMediaPlanTermDTO(int xmptermid, int xmpid, DateOnly date, string? spotcode) 
            : base(xmptermid, xmpid, date, spotcode)
        {
        }
    }
}
