using Database.DTOs.SeasonalitiesDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ISeasonalitiesRepository
    {
        Task<bool> CreateSeasonalities(CreateSeasonalitiesDTO seasonalitiesDTO);
        Task<IEnumerable<SeasonalitiesDTO>> GetSeasonalitiesById(int id);
        Task<IEnumerable<SeasonalitiesDTO>> GetAllSeasonalities();
        Task<bool> UpdateSeasonalities(UpdateSeasonalitiesDTO seasonalitiesDTO);
        Task<bool> DeleteSeasonalitiesById(int id);
    }
}
