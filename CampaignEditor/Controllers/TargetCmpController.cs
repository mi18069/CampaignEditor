using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Database.DTOs.TargetCmpDTO;

namespace CampaignEditor.Controllers
{
    public class TargetCmpController : ControllerBase
    {
        private readonly ITargetCmpRepository _repository;
        public TargetCmpController(ITargetCmpRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<TargetCmpDTO> CreateTargetCmp(CreateTargetCmpDTO targetCmpDTO)
        {
            await _repository.CreateTargetCmp(targetCmpDTO);
            return await _repository.GetTargetCmpByIds(targetCmpDTO.cmpid, targetCmpDTO.targid);
        }
        public async Task<TargetCmpDTO> GetTargetCmpByIds(int cmpid, int targid)
        {
            return await _repository.GetTargetCmpByIds(cmpid, targid);
        }

        public async Task<IEnumerable<TargetCmpDTO>> GetTargetCmpByCmpid(int id)
        {
            return await _repository.GetTargetCmpByCmpid(id);
        }

        public async Task<IEnumerable<TargetCmpDTO>> GetTargetCmpExceptCmpid(int id)
        {
            return await _repository.GetTargetCmpExceptCmpid(id);
        }

        public async Task<IEnumerable<TargetCmpDTO>> GetAllTargetCmps()
        {
            return await _repository.GetAllTargetCmps();
        }

        public async Task<bool> UpdateTargetCmp(UpdateTargetCmpDTO targetCmpDTO)
        {
            return await _repository.UpdateTargetCmp(targetCmpDTO);
        }

        public async Task<bool> DeleteTargetCmpByCmpid(int id)
        {
            return await _repository.DeleteTargetCmpByCmpid(id);
        }

        public async Task<bool> DeleteSpotByIds(int cmpid, int targid)
        {
            return await _repository.DeleteTargetCmpByIds(cmpid, targid);
        }
    }
}
