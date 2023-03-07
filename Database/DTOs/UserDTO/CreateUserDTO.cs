
namespace CampaignEditor.DTOs.UserDTO
{
    public class CreateUserDTO : BaseUserDTO
    {
        public CreateUserDTO(string usrname, string usrpass, int usrlevel, string email, string telefon, int enabled, int father, bool buy) 
            : base(usrname, usrpass, usrlevel, email, telefon, enabled, father, buy)
        {
        }
    }
}
