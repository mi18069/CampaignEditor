
namespace Database.DTOs.SeasonalitiesDTO
{
    public class SeasonalitiesDTO : BaseIdentitySeasonalitiesDTO
    {

        public SeasonalitiesDTO(int seasid, string stdt, string endt, decimal coef) 
            : base(seasid, stdt, endt, coef)
        {
        }
    }
}
