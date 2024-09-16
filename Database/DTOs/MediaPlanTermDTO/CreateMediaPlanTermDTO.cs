using Database.Entities;
using System;

namespace Database.DTOs.MediaPlanTermDTO
{
    public class CreateMediaPlanTermDTO : BaseMediaPlanTermDTO
    {
        public CreateMediaPlanTermDTO(int xmpid, DateOnly date, string? spotcode, string? added, string? deleted) 
            : base(xmpid, date, spotcode, added, deleted)
        {
        }
        public CreateMediaPlanTermDTO(MediaPlanTermDTO mpTermDTO)
            : base(mpTermDTO.xmpid, mpTermDTO.date, mpTermDTO.spotcode, mpTermDTO.added, mpTermDTO.deleted)
        {
        }

        public CreateMediaPlanTermDTO(MediaPlanTerm mpTerm)
        : base(mpTerm.Xmpid, mpTerm.Date, mpTerm.Spotcode, mpTerm.Added, mpTerm.Deleted)
        {
        }
    }
}
