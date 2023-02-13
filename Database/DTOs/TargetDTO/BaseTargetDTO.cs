using System;

namespace Database.DTOs.TargetDTO
{
    public class BaseTargetDTO
    {
        public BaseTargetDTO(string targname, int targown, string targdesc, string targdefi, string targdefp)
        {
            this.targname = targname ?? throw new ArgumentNullException(nameof(targname));
            this.targown = targown;
            this.targdesc = targdesc ?? throw new ArgumentNullException(nameof(targdesc));
            this.targdefi = targdefi ?? throw new ArgumentNullException(nameof(targdefi));
            this.targdefp = targdefp ?? throw new ArgumentNullException(nameof(targdefp));
        }

        public string targname { get; set; }
        public int targown { get; set; }
        public string targdesc { get; set; }
        public string targdefi { get; set; }
        public string targdefp { get; set; }
    }
}
