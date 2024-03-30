
using Database.DTOs.DayPartDTO;
using Database.DTOs.TargetDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IDayPartRepository
    {
        Task<bool> CreateDayPart(CreateDayPartDTO dayPartDTO);
        Task<DayPartDTO> GetDayPartById(int id);
        Task<IEnumerable<DayPartDTO>> GetAllClientDayParts(int clientId);
        Task<bool> UpdateDayPart(UpdateDayPartDTO dayPartDTO);
        Task<bool> DeleteDayPart(int id);
    }
}
