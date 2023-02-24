using Database.DTOs.SeasonalityDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ISeasonalityRepository
    {
        Task<bool> CreateSeasonality(CreateSeasonalityDTO seasonalityDTO);
        Task<SeasonalityDTO> GetSeasonalityById(int id);
        Task<SeasonalityDTO> GetSeasonalityByName(string seasonalityname);
        Task<IEnumerable<SeasonalityDTO>> GetAllSeasonalities();
        Task<bool> UpdateSeasonality(UpdateSeasonalityDTO seasonalityDTO);
        Task<bool> DeleteSeasonalityById(int id);
    }
}
