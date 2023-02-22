using Database.DTOs.SectablesDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ISectablesRepository
    {
        Task<bool> CreateSectables(CreateSectablesDTO sectablesDTO);
        Task<IEnumerable<SectablesDTO>> GetSectablesById(int id);
        Task<IEnumerable<SectablesDTO>> GetAllSectables();
        Task<bool> UpdateSectables(UpdateSectablesDTO sectablesDTO);
        Task<bool> DeleteSectablesById(int id);
    }
}
