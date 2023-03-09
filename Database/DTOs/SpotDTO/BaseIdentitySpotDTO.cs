
using System;

namespace Database.DTOs.SpotDTO
{
    public class BaseIdentitySpotDTO
    {
        public BaseIdentitySpotDTO(int cmpid, string spotcode, string spotname, int spotlength, bool ignore)
        {
            this.cmpid = cmpid;
            this.spotcode = spotcode ?? throw new ArgumentNullException(nameof(spotcode));
            this.spotname = spotname ?? throw new ArgumentNullException(nameof(spotname));
            this.spotlength = spotlength;
            this.ignore = ignore;
        }

        public int cmpid { get; set; }
        public string spotcode { get; set; }
        public string spotname { get; set; }
        public int spotlength { get; set; }
        public bool ignore { get; set; }
    }
}
