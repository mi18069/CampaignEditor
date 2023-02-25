using Database.DTOs.SectableDTO;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class SectableController : ControllerBase
    {
        private readonly ISectableRepository _repository;
        public SectableController(ISectableRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<SectableDTO> CreateSectable(CreateSectableDTO sectableDTO)
        {
            await _repository.CreateSectable(sectableDTO);
            var channel = await _repository.GetSectableByName(sectableDTO.sctname);
            return channel;
        }

        public async Task<SectableDTO> GetSectableById(int id)
        {
            var sectable = await _repository.GetSectableById(id);
            return sectable;
        }
        public async Task<SectableDTO> GetSectableByName(string sectablename)
        {
            var sectable = await _repository.GetSectableByName(sectablename);
            return sectable;
        }
        public async Task<IEnumerable<SectableDTO>> GetAllSectablesByOwnerId(int id)
        {
            return await _repository.GetAllSectablesByOwnerId(id);
        }
        public async Task<IEnumerable<SectableDTO>> GetAllSectables()
        {
            return await _repository.GetAllSectables();
        }

        public async Task<bool> UpdateSectable(UpdateSectableDTO sectableDTO)
        {
            return await _repository.UpdateSectable(sectableDTO);
        }

        public async Task<bool> DeleteSectableById(int id)
        {
            return await _repository.DeleteSectableById(id);
        }
    }
}
