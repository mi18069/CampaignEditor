using System;

namespace Database.DTOs.SectableDTO
{
    public class BaseSectableDTO
    {
        public BaseSectableDTO(string sctname, bool sctlinear, bool sctactive, int ownedby)
        {
            this.sctname = sctname ?? throw new ArgumentNullException(nameof(sctname));
            this.sctlinear = sctlinear;
            this.sctactive = sctactive;
            this.ownedby = ownedby;
        }

        public string sctname { get; set; }
        public bool sctlinear { get; set; }
        public bool sctactive { get; set; }
        public int ownedby { get; set; }
    }
}
