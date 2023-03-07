
namespace Database.DTOs.SeasonalitiesDTO
{
    public class CreateSeasonalitiesDTO : BaseIdentitySeasonalitiesDTO
    {
        public CreateSeasonalitiesDTO(int seasid, string stdt, string endt, double coef) 
            : base(seasid, stdt, endt, coef)
        {
        }
    }
}
