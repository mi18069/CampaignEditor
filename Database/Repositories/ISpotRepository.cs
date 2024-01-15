using Database.DTOs.SpotDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ISpotRepository
    {
        Task<bool> CreateSpot(CreateSpotDTO spotDTO);
        Task<IEnumerable<SpotDTO>> GetSpotsByCmpid(int id);
        Task<SpotDTO> GetSpotByCmpidAndCode(int id, string code);
        Task<SpotDTO> GetSpotByName(string spotname);
        Task<IEnumerable<SpotDTO>> GetAllSpots();
        Task<bool> UpdateSpot(UpdateSpotDTO spotDTO);
        Task<bool> DeleteSpotsByCmpid(int id);
        Task<bool> DeleteSpotByCmpidAndCode(int id, string code);
        Task<bool> DuplicateSpot(int oldId, int newId);
    }
}
