using System;

namespace Database.DTOs.SeasonalitiesDTO
{
    public class BaseIdentitySeasonalitiesDTO
    {
        public BaseIdentitySeasonalitiesDTO(int seasid, string stdt, string endt, decimal coef)
        {
            this.seasid = seasid;
            this.stdt = stdt ?? throw new ArgumentNullException(nameof(stdt));
            this.endt = endt ?? throw new ArgumentNullException(nameof(endt));
            this.coef = coef;
        }

        public int seasid { get; set; }
        public string stdt { get; set; }
        public string endt { get; set; }
        public decimal coef { get; set; }
    }
}
