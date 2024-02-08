using Database.DTOs.MediaPlanDTO;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Database.Repositories;

namespace CampaignEditor.Controllers
{
    public class MediaPlanController : ControllerBase
    {
        private readonly IMediaPlanRepository _repository;
        public MediaPlanController(IMediaPlanRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<MediaPlanDTO> CreateMediaPlan(CreateMediaPlanDTO mediaPlanDTO)
        {
            await _repository.CreateMediaPlan(mediaPlanDTO);
            return await _repository.GetMediaPlanBySchemaAndCmpId(mediaPlanDTO.schid, mediaPlanDTO.cmpid, mediaPlanDTO.version);
        }

        public async Task<MediaPlanDTO> CreateAndReturnMediaPlan(CreateMediaPlanDTO mediaPlanDTO)
        {
            var mpDTO = await _repository.CreateAndReturnMediaPlan(mediaPlanDTO);
            return mpDTO;
        }

        public async Task<MediaPlanDTO> GetMediaPlanById(int id)
        {
            return await _repository.GetMediaPlanById(id);
        }

        public async Task<MediaPlanDTO> GetMediaPlanBySchemaId(int id)
        {
            return await _repository.GetMediaPlanBySchemaId(id);
        }

        public async Task<MediaPlanDTO?> GetMediaPlanBySchemaAndCmpId(int schemaid, int cmpid, int version = 1)
        {
            return await _repository.GetMediaPlanBySchemaAndCmpId(schemaid, cmpid, version);
        }

        public async Task<MediaPlanDTO> GetMediaPlanByCmpId(int id)
        {
            return await _repository.GetMediaPlanByCmpId(id);
        }

        public async Task<MediaPlanDTO> GetMediaPlanByName(string name)
        {
            return await _repository.GetMediaPlanByName(name);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlans()
        {
            return await _repository.GetAllMediaPlans();
        }

        public async Task<IEnumerable<int>> GetAllChannelsByCmpid(int cmpid, int version = 1)
        {
            return await _repository.GetAllChannelsByCmpid(cmpid, version);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansByCmpid(int cmpid, int version = 1)
        {
            return await _repository.GetAllMediaPlansByCmpid(cmpid, version);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansByCmpidAndChannel(int cmpid, int chid, int version = 1)
        {
            return await _repository.GetAllMediaPlansByCmpidAndChannel(cmpid, chid, version);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansWithinDate(DateOnly sdate, DateOnly edate)
        {
            return await _repository.GetAllMediaPlansWithinDate(sdate, edate);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllChannelCmpMediaPlans(int chid, int cmpid, int version = 1)
        {
            return await _repository.GetAllChannelCmpMediaPlans(chid, cmpid, version);
        }

        public async Task<bool> UpdateMediaPlan(UpdateMediaPlanDTO mediaPlanDTO)
        {
            return await _repository.UpdateMediaPlan(mediaPlanDTO);
        }

        public async Task<bool> DeleteMediaPlanById(int id)
        {
            return await _repository.DeleteMediaPlanById(id);
        }

        public async Task<bool> DeleteMediaPlanByCmpId(int id)
        {
            return await _repository.DeleteMediaPlanByCmpId(id);
        }

        public async Task<bool> SetActiveMediaPlanById(int id, bool isActive)
        {
            return await _repository.SetActiveMediaPlanById(id, isActive);
        }
    }
}
