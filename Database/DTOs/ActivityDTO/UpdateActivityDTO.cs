
namespace Database.DTOs.ActivityDTO
{
    public class UpdateActivityDTO : BaseIdentityActivityDTO
    {
        public UpdateActivityDTO(int actid, string act) 
            : base(actid, act)
        {
        }
    }
}
