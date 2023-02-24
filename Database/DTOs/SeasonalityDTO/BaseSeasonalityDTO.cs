using System;

namespace Database.DTOs.SeasonalityDTO
{
    public class BaseSeasonalityDTO
    {
        public BaseSeasonalityDTO(string seasname, bool seasactive, string channel, int ownedby)
        {
            this.seasname = seasname ?? throw new ArgumentNullException(nameof(seasname));
            this.seasactive = seasactive;
            this.channel = channel;
            this.ownedby = ownedby;
        }

        public string seasname { get; set; }
        public bool seasactive { get; set; }
        public string channel { get; set; }
        public int ownedby { get; set; }
    }
}
