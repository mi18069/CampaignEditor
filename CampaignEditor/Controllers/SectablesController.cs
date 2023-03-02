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

        // Searches for id and sec, if it isn't found, create new, if it's found update it
        public async Task<SectablesDTO> CreateOrUpdateSectablesByIdAndSec(int id, int sec, double coef)
        {
            var sectables = await _repository.GetSectablesByIdAndSec(id, sec);
            if (sectables == null)
            {
                await _repository.CreateSectables(new CreateSectablesDTO(id, sec, coef));
            }
            else
            {
                await _repository.UpdateSectables(new UpdateSectablesDTO(id, sec, coef));
            }
            return await _repository.GetSectablesByIdAndSec(id, sec);
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
