using Database.DTOs.DPTimeDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class DPTimeController
    {
        private readonly IDPTimeRepository _repository;
        public DPTimeController(IDPTimeRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<DPTimeDTO> CreateDPTime(CreateDPTimeDTO dpTimeDTO)
        {
            int id = await _repository.CreateDPTime(dpTimeDTO);
            return await _repository.GetDPTimeById(id);
        }

        public async Task<DPTimeDTO> GetDPTimeById(int id)
        {
            return await _repository.GetDPTimeById(id);
        }

        public async Task<IEnumerable<DPTimeDTO>> GetAllDPTimesByDPId(int dpId)
        {
            return await _repository.GetAllDPTimesByDPId(dpId);
        }

        public async Task<bool> UpdateDPTime(UpdateDPTimeDTO dpTimeDTO)
        {
            return await _repository.UpdateDPTime(dpTimeDTO);
        }

        public async Task<bool> DeleteDPTime(int id)
        {
            return await _repository.DeleteDPTime(id);
        }

        public async Task<bool> DeleteDPTimeByDPId(int id)
        {
            return await _repository.DeleteDPTimeByDPId(id);
        }
    }
}
