
namespace CampaignEditor.DTOs.UserDTO
{
    public class UpdateUserDTO : BaseIdentityUserDTO
    {
        public UpdateUserDTO(int usrid, string usrname, string usrpass, int usrlevel, string email, string telefon, int enabled, int father, bool buy) 
            : base(usrid, usrname, usrpass, usrlevel, email, telefon, enabled, father, buy)
        {
        }
        public UpdateUserDTO(UserDTO userDTO)
            : base(userDTO.usrid, userDTO.usrname, userDTO.usrpass, userDTO.usrlevel, userDTO.email, userDTO.telefon, userDTO.enabled, userDTO.father, userDTO.buy)
        {
        }
    }
}
