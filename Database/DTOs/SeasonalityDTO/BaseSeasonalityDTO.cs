using System;

namespace Database.DTOs.SeasonalityDTO
{
    public class BaseSeasonalityDTO
    {
        public BaseSeasonalityDTO(string seasname, bool seasactive, int ownedby)
        {
            this.seasname = seasname ?? throw new ArgumentNullException(nameof(seasname));
            this.seasactive = seasactive;
            this.ownedby = ownedby;
        }

        public string seasname { get; set; }
        public bool seasactive { get; set; }
        public int ownedby { get; set; }
    }
}
