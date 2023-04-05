using System;

namespace Database.Entities
{
    public class MediaPlanTerm
    {
        public int xmptermid { get; set; }
        public int xmpid { get; set; }
        public DateOnly date { get; set; }
        public string spotcode { get; set; }
    }
}
