using Database.DTOs.MediaPlanRealizedDTO;
using Database.Entities;
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

        public async Task<IEnumerable<MediaPlanRealized>> GetAllMediaPlansRealizedByCmpid(int cmpid)
        {
            return await _repository.GetAllMediaPlansRealizedByCmpid(cmpid);

        }

        public async Task<IEnumerable<MediaPlanRealizedDTO>> GetAllMediaPlansRealizedByChid(int chid)
        {
            return await _repository.GetAllMediaPlansRealizedByChid(chid);

        }

        public async Task<IEnumerable<MediaPlanRealizedDTO>> GetAllMediaPlansRealizedByChidAndDate(int chid, string date)
        {
            return await _repository.GetAllMediaPlansRealizedByChidAndDate(chid, date);
        }

        public async Task<IEnumerable<MediaPlanRealizedDTO>> GetAllMediaPlansRealizedByCmpidAndEmsnum(int cmpid, int emsnum)
        {
            return await _repository.GetAllMediaPlansRealizedByCmpidAndEmsnum(cmpid, emsnum);
        }

        public async Task<bool> UpdateMediaPlanRealized(UpdateMediaPlanRealizedDTO mediaPlanRealizedDTO)
        {
            return await _repository.UpdateMediaPlanRealized(mediaPlanRealizedDTO);
        }
        public async Task<bool> UpdateMediaPlanRealized(MediaPlanRealized mediaPlanRealized)
        {
            return await _repository.UpdateMediaPlanRealized(mediaPlanRealized);
        }

        public async Task<bool> DeleteMediaPlanRealizedById(int id)
        {
            return await _repository.DeleteMediaPlanRealizedById(id);
        }

        public async Task<string> GetDedicatedSpotName(int spotid)
        {
            return await _repository.GetDedicatedSpotName(spotid);
        }

        public async Task<List<Tuple<int, string>>> GetAllSpotNumSpotNamePairs(int spotid)
        {
            return await _repository.GetAllSpotNumSpotNamePairs(spotid);
        }

        public async Task<bool> SetStatusValue(int id, int statusValue)
        {
            return await _repository.SetStatusValue(id, statusValue);
        }
    }
}
