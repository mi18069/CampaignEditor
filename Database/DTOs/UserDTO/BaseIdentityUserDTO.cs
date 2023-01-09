using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor.DTOs.UserDTO
{
    public class BaseIdentityUserDTO : BaseUserDTO
    {
        public BaseIdentityUserDTO(string usrname, string usrpass, int usrlevel, string email, string telefon, int enabled, int father, bool buy) 
            : base(usrname, usrpass, usrlevel, email, telefon, enabled, father, buy)
        {
        }

        public int usrid { get; set; }
    }
}
