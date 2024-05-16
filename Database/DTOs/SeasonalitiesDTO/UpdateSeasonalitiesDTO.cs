
namespace Database.DTOs.SeasonalitiesDTO
{
    public class UpdateSeasonalitiesDTO : BaseIdentitySeasonalitiesDTO
    {
        public UpdateSeasonalitiesDTO(int seasid, string stdt, string endt, decimal coef) 
            : base(seasid, stdt, endt, coef)
        {
        }
    }
}
