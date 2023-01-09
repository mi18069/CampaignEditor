namespace CampaignEditor.DTOs.UserDTO
{
    public class UserDTO : BaseIdentityUserDTO
    {
        public UserDTO(string usrname, string usrpass, int usrlevel, string email, string telefon, int enabled, int father, bool buy) 
            : base(usrname, usrpass, usrlevel, email, telefon, enabled, father, buy)
        {
        }
    }
}
