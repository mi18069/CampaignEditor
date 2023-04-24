using System;

namespace Database.DTOs.MediaPlanRef
{
    public class BaseMediaPlanRefDTO
    {
        public BaseMediaPlanRefDTO(int cmpid, DateOnly datefrom, DateOnly dateto, int version)
        {
            this.cmpid = cmpid;
            this.datefrom = datefrom;
            this.dateto = dateto;
            this.version = version;
        }

        public int cmpid { get; set; }
        public DateOnly datefrom { get; set; }
        public DateOnly dateto { get; set; }
        public int version { get; set; }
    }
}
