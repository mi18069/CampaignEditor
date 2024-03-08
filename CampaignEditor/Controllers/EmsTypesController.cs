using Database.DTOs.EmsTypesDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class EmsTypesController : ControllerBase
    {
        private readonly IEmsTypesRepository _repository;
        public EmsTypesController(IEmsTypesRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<EmsTypesDTO> GetEmsTypesByCode(string code)
        {
            return await _repository.GetEmsTypesByCode(code);
        }

        public async Task<IEnumerable<EmsTypesDTO>> GetAllEmsTypes()
        {
            return await _repository.GetAllEmsTypes();
        }

        public async Task<IEnumerable<EmsTypes>> GetAllEmsTypesEntities()
        {
            return await _repository.GetAllEmsTypesEntities();
        }

    }
}
