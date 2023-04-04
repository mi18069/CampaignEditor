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
        public async Task<bool> CreateMediaPlan(CreateMediaPlanDTO mediaPlanDTO)
        {
            return await _repository.CreateMediaPlan(mediaPlanDTO);

        }

        public async Task<MediaPlanDTO> GetMediaPlanById(int id)
        {
            return await _repository.GetMediaPlanById(id);
        }

        public async Task<MediaPlanDTO> GetMediaPlanByName(string name)
        {
            return await _repository.GetMediaPlanByName(name);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlans()
        {
            return await _repository.GetAllMediaPlans();
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansWithinDate(DateOnly sdate, DateOnly edate)
        {
            return await _repository.GetAllMediaPlansWithinDate(sdate, edate);
        }

        public async Task<IEnumerable<MediaPlanDTO>> GetAllChannelMediaPlans(int chid)
        {
            return await _repository.GetAllChannelMediaPlans(chid);
        }

        public async Task<bool> UpdateMediaPlan(UpdateMediaPlanDTO mediaPlanDTO)
        {
            return await _repository.UpdateMediaPlan(mediaPlanDTO);
        }

        public async Task<bool> DeleteMediaPlanById(int id)
        {
            return await _repository.DeleteMediaPlanById(id);
        }
    }
}
