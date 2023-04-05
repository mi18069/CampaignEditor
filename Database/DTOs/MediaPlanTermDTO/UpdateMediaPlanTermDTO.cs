using System;

namespace Database.DTOs.MediaPlanTermDTO
{
    public class UpdateMediaPlanTermDTO : BaseIdentityMediaPlanTermDTO
    {
        public UpdateMediaPlanTermDTO(int cmptermid, int xmpid, DateOnly date, string spotcode) 
            : base(cmptermid, xmpid, date, spotcode)
        {
        }
    }
}
