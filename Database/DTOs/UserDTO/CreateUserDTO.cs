using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

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
