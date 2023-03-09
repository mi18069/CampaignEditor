
namespace Database.DTOs.TargetCmpDTO
{
    public class BaseIdentityTargetCmpDTO
    {
        public BaseIdentityTargetCmpDTO(int cmpid, int targid, int priority)
        {
            this.cmpid = cmpid;
            this.targid = targid;
            this.priority = priority;
        }

        public int cmpid { get; set; }
        public int targid { get; set; }
        public int priority { get; set; }
    }
}
