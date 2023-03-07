
namespace CampaignEditor.DTOs.UserDTO
{
    public class UpdateUserDTO : BaseIdentityUserDTO
    {
        public UpdateUserDTO(int usrid, string usrname, string usrpass, int usrlevel, string email, string telefon, int enabled, int father, bool buy) 
            : base(usrid, usrname, usrpass, usrlevel, email, telefon, enabled, father, buy)
        {
        }
    }
}
