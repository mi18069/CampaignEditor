using Database.DTOs.MediaPlanRealizedDTO;
using Database.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class MediaPlanRealizedController
    {
        private readonly IMediaPlanRealizedRepository _repository;
        public MediaPlanRealizedController(IMediaPlanRealizedRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> CreateMediaPlanRealized(CreateMediaPlanRealizedDTO mediaPlanRealizedDTO)
        {
            return await _repository.CreateMediaPlanRealized(mediaPlanRealizedDTO);
        }

        public async Task<MediaPlanRealizedDTO> GetMediaPlanRealizedById(int id)
        {
            return await _repository.GetMediaPlanRealizedById(id);
        }

        public async Task<IEnumerable<MediaPlanRealizedDTO>> GetAllMediaPlansRealizedByChid(int chid)
        {
            return await _repository.GetAllMediaPlansRealizedByChid(chid);

        }

        public async Task<IEnumerable<MediaPlanRealizedDTO>> GetAllMediaPlansRealizedByChidAndDate(int chid, string date)
        {
            return await _repository.GetAllMediaPlansRealizedByChidAndDate(chid, date);
        }

        public async Task<bool> UpdateMediaPlanRealized(UpdateMediaPlanRealizedDTO mediaPlanRealizedDTO)
        {
            return await _repository.UpdateMediaPlanRealized(mediaPlanRealizedDTO);
        }

        public async Task<bool> DeleteMediaPlanRealizedById(int id)
        {
            return await _repository.DeleteMediaPlanRealizedById(id);
        }
    }
}
