﻿
namespace Database.DTOs.TargetDTO
{
    public class CreateTargetDTO : BaseTargetDTO
    {
        public CreateTargetDTO(string targname, int targown, string targdesc, string targdefi, string targdefp) 
            : base(targname, targown, targdesc, targdefi, targdefp)
        {
        }
    }
}
