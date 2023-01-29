using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.DTOs.TargetClassDTO
{
    public class CreateTargetClassDTO : BaseTargetClassDTO
    {
        public CreateTargetClassDTO(string name, int type, string position) 
            : base(name, type, position)
        {
        }
    }
}
