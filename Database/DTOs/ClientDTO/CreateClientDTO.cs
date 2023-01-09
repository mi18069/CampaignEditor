using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DTOs.ClientDTO
{
    public class CreateClientDTO : BaseClientDTO
    {
        public CreateClientDTO(string clname, bool clactive, int spid) 
            : base(clname, clactive, spid)
        {
        }
    }
}
