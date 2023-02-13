namespace CampaignEditor.DTOs.UserDTO
{
    public class UserDTO : BaseIdentityUserDTO
    {
        public UserDTO(int usrid, string usrname, string usrpass, int usrlevel, string email, string telefon, int enabled, int father, bool buy) 
            : base(usrid, usrname, usrpass, usrlevel, email, telefon, enabled, father, buy)
        {
        }
    }
}
