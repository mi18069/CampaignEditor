using Database.DTOs.SectableChannels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ISectableChannelsRepository
    {
        Task<bool> CreateSectableChannels(CreateSectableChannelsDTO sectableChannelsDTO);
        Task<SectableChannelsDTO> GetSectableChannelsByIds(int sctid, int chid);
        Task<IEnumerable<SectableChannelsDTO>> GetAllSectableChannelsBySctid(int sctid);
        Task<IEnumerable<SectableChannelsDTO>> GetAllSectableChannelsByChid(int chid);
        Task<IEnumerable<SectableChannelsDTO>> GetAllSectableChannels();
        Task<bool> UpdateSectableChannels(UpdateSectableChannelsDTO sectableChannelsDTO);
        Task<bool> DeleteSectableChannelsByIds(int sctid, int chid);
    }
}
