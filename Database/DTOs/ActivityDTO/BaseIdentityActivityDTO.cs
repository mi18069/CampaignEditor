
namespace Database.DTOs.ActivityDTO
{
    public class BaseIdentityActivityDTO : BaseActivityDTO
    {
        public BaseIdentityActivityDTO(int actid, string act) 
            : base(act)
        {
            this.actid = actid;
        }

        public int actid { get; set; }
    }
}
