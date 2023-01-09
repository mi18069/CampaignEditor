using Database.DTOs.ClientDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DTOs.ClientDTO
{
    public class BaseIdentityClientDTO : BaseClientDTO
    {
        public BaseIdentityClientDTO(string clname, bool clactive, int spid) 
            : base(clname, clactive, spid)
        {
        }
        public int clid { get; set; }
    }
}
