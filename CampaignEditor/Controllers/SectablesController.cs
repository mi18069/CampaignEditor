using Database.DTOs.SectablesDTO;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class SectablesController : ControllerBase
    {
        private readonly ISectablesRepository _repository;
        public SectablesController(ISectablesRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> CreateSectables(CreateSectablesDTO sectablesDTO)
        {
            return await _repository.CreateSectables(sectablesDTO);
        }

        public async Task<IEnumerable<SectablesDTO>> GetSectablesById(int id)
        {
            var sectables = await _repository.GetSectablesById(id);
            return sectables;
        }

        public async Task<IEnumerable<SectablesDTO>> GetAllSectables()
        {
            return await _repository.GetAllSectables();
        }

        public async Task<bool> UpdateSectables(UpdateSectablesDTO sectablesDTO)
        {
            return await _repository.UpdateSectables(sectablesDTO);
        }

        public async Task<bool> DeleteSectablesById(int id)
        {
            return await _repository.DeleteSectablesById(id);
        }
    }
}
