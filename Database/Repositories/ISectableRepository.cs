using Database.DTOs.SectableDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ISectableRepository
    {
        Task<bool> CreateSectable(CreateSectableDTO sectableDTO);
        Task<SectableDTO> GetSectableById(int id);
        Task<SectableDTO> GetSectableByName(string sectablename);
        Task<IEnumerable<SectableDTO>> GetAllSectablesByOwnerId(int id);
        Task<IEnumerable<SectableDTO>> GetAllSectables();
        Task<bool> UpdateSectable(UpdateSectableDTO sectableDTO);
        Task<bool> DeleteSectableById(int id);
    }
}
