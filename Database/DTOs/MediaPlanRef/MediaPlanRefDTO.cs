using System;

namespace Database.DTOs.MediaPlanRef
{
    public class MediaPlanRefDTO : BaseMediaPlanRefDTO
    {
        public MediaPlanRefDTO(int cmpid, DateOnly datefrom, DateOnly dateto, int version) 
            : base(cmpid, datefrom, dateto, version)
        {
        }
    }
}
