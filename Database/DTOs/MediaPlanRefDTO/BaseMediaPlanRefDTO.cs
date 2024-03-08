namespace Database.DTOs.MediaPlanRef
{
    public class BaseMediaPlanRefDTO
    {
        public BaseMediaPlanRefDTO(int cmpid, int datestart, int dateend)
        {
            this.cmpid = cmpid;
            this.datestart = datestart;
            this.dateend = dateend;
        }

        public int cmpid { get; set; }
        public int datestart { get; set; }
        public int dateend { get; set; }
    }
}
