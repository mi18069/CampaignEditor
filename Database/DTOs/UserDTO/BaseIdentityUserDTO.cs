
namespace CampaignEditor.DTOs.UserDTO
{
    public class BaseIdentityUserDTO : BaseUserDTO
    {
        public BaseIdentityUserDTO(int usrid, string usrname, string usrpass, int usrlevel, string email, string telefon, int enabled, int father, bool buy) 
            : base(usrname, usrpass, usrlevel, email, telefon, enabled, father, buy)
        {
            this.usrid = usrid;
        }

        public int usrid { get; set; }
    }
}
