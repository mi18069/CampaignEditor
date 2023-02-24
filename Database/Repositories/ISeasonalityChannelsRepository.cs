using Database.DTOs.SeasonalityChannelsDTO;
using Database.DTOs.SectableChannels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface ISeasonalityChannelsRepository
    {
        Task<bool> CreateSeasonalityChannels(CreateSeasonalityChannelsDTO seasonalityChannelsDTO);
        Task<SeasonalityChannelsDTO> GetSeasonalityChannelsByIds(int seasid, int chid);
        Task<IEnumerable<SeasonalityChannelsDTO>> GetAllSeasonalityChannelsBySeasid(int seasid);
        Task<IEnumerable<SeasonalityChannelsDTO>> GetAllSeasonalityChannelsByChid(int chid);
        Task<IEnumerable<SeasonalityChannelsDTO>> GetAllSeasonalityChannels();
        Task<bool> UpdateSeasonalityChannels(UpdateSeasonalityChannelsDTO seasonalityChannelsDTO);
        Task<bool> DeleteSeasonalityChannelsByIds(int seasid, int chid);
    }
}
