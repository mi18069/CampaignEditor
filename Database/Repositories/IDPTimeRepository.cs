using Database.DTOs.DPTimeDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IDPTimeRepository
    {
        Task<int> CreateDPTime(CreateDPTimeDTO dpTimeDTO);
        Task<DPTimeDTO> GetDPTimeById(int id);
        Task<IEnumerable<DPTimeDTO>> GetAllDPTimesByDPId(int dpId);
        Task<bool> UpdateDPTime(UpdateDPTimeDTO dpTimeDTO);
        Task<bool> DeleteDPTime(int id);
        Task<bool> DeleteDPTimeByDPId(int id);
    }
}
