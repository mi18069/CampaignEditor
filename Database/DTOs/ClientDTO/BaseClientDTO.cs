using System;

namespace Database.DTOs.ClientDTO
{
    public class BaseClientDTO
    {
        public BaseClientDTO(string clname, bool clactive, int spid)
        {
            this.clname = clname ?? throw new ArgumentNullException(nameof(clname));
            this.clactive = clactive;
            this.spid = spid;
        }

        public string clname { get; set; }
        public bool clactive { get; set; }
        public int spid { get; set; }
    }
}
