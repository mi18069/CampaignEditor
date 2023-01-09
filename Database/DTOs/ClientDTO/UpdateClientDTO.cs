using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DTOs.ClientDTO
{
    public class UpdateClientDTO : BaseIdentityClientDTO
    {
        public UpdateClientDTO(string clname, bool clactive, int spid) 
            : base(clname, clactive, spid)
        {
        }
    }
}
