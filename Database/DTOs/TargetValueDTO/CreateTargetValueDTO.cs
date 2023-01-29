using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DTOs.TargetValueDTO
{
    public class CreateTargetValueDTO : BaseTargetValueDTO
    {
        public CreateTargetValueDTO(string name, int value) 
            : base(name, value)
        {
        }
    }
}
