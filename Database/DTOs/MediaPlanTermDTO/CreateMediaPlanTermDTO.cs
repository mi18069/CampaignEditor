using System;

namespace Database.DTOs.MediaPlanTermDTO
{
    public class CreateMediaPlanTermDTO : BaseMediaPlanTermDTO
    {
        public CreateMediaPlanTermDTO(int xmpid, DateOnly date, string? spotcode) 
            : base(xmpid, date, spotcode)
        {
        }
        public CreateMediaPlanTermDTO(MediaPlanTermDTO mpTermDTO)
            : base(mpTermDTO.xmpid, mpTermDTO.date, mpTermDTO.spotcode)
        {
        }
    }
}
