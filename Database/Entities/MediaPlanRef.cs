using System;

namespace Database.Entities
{
    public class MediaPlanRef
    {
        public int cmpid { get; set; }
        public DateOnly datefrom { get; set; }
        public DateOnly dateto { get; set; }
        public int version { get; set; }
    }
}
