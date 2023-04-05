using System;

namespace Database.DTOs.MediaPlanTermDTO
{
    public class MediaPlanTermDTO : BaseIdentityMediaPlanTermDTO
    {
        public MediaPlanTermDTO(int cmptermid, int xmpid, DateOnly date, string spotcode) 
            : base(cmptermid, xmpid, date, spotcode)
        {
        }
    }
}
