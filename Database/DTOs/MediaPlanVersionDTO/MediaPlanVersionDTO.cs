
namespace Database.DTOs.MediaPlanVersionDTO
{
    public class MediaPlanVersionDTO
    {
        public MediaPlanVersionDTO(int cmpid, int version)
        {
            this.cmpid = cmpid;
            this.version = version;
        }

        public int cmpid { get; set; }
        public int version { get; set; }
    }
}
