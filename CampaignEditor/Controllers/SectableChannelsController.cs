using Database.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Database.DTOs.SectableChannels;
using Microsoft.AspNetCore.Mvc;

namespace CampaignEditor.Controllers
{
    public class SectableChannelsController : ControllerBase
    {
        private readonly ISectableChannelsRepository _repository;
        public SectableChannelsController(ISectableChannelsRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<bool> CreateSectableChannels(CreateSectableChannelsDTO sectableChannelsDTO)
        {
            return await _repository.CreateSectableChannels(sectableChannelsDTO);
        }

        public async Task<SectableChannelsDTO> GetSectableChannelsByIds(int sctid, int chid)
        {
            var sectableChannels = await _repository.GetSectableChannelsByIds(sctid, chid);
            return sectableChannels;
        }

        public async Task<IEnumerable<SectableChannelsDTO>> GetAllSectableChannelsBySctid(int sctid)
        {
            return await _repository.GetAllSectableChannelsBySctid(sctid);
        }

        public async Task<IEnumerable<SectableChannelsDTO>> GetAllSectableChannelsByChid(int chid)
        {
            return await _repository.GetAllSectableChannelsByChid(chid);
        }

        public async Task<IEnumerable<SectableChannelsDTO>> GetAllSectableChannels()
        {
            return await _repository.GetAllSectableChannels();
        }

        public async Task<bool> UpdateSectableChannels(UpdateSectableChannelsDTO sectableChannelsDTO)
        {
            return await _repository.UpdateSectableChannels(sectableChannelsDTO);
        }

        public async Task<bool> DeleteSectableChannelsByIds(int sctid, int chid)
        {
            return await _repository.DeleteSectableChannelsByIds(sctid, chid);
        }
    }
}
