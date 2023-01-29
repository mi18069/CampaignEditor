using Database.DTOs.TargetClassDTO;
using Database.DTOs.TargetValueDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ITargetValueRepository
    {
        Task<bool> CreateTargetValue(CreateTargetValueDTO targetValueDTO);
        Task<TargetValueDTO> GetTargetValueById(int id);
        Task<TargetValueDTO> GetTargetValueByName(string valuename);
        Task<IEnumerable<TargetValueDTO>> GetAllTargetValues();
        Task<bool> UpdateTargetValue(UpdateTargetValueDTO targetValueDTO);
        Task<bool> DeleteTargetValueById(int id);
        Task<bool> DeleteTargetValueByName(string valuename);
    }
}
