using System;

namespace Database.DTOs.MediaPlanTermDTO
{
    public class UpdateMediaPlanTermDTO : BaseIdentityMediaPlanTermDTO
    {
        public UpdateMediaPlanTermDTO(int xmptermid, int xmpid, DateOnly date, string? spotcode, string? added, string? deleted) 
            : base(xmptermid, xmpid, date, spotcode, added, deleted)
        {
        }

        public UpdateMediaPlanTermDTO(MediaPlanTermDTO mpTermDTO)
            : base(mpTermDTO.xmptermid, mpTermDTO.xmpid, mpTermDTO.date, mpTermDTO.spotcode, mpTermDTO.added, mpTermDTO.deleted)
        {
        }
    }
}
