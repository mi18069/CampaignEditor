using Database.DTOs.MediaPlanRef;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class MediaPlanRefController : ControllerBase
    {
        private readonly IMediaPlanRefRepository _repository;
        public MediaPlanRefController(IMediaPlanRefRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<MediaPlanRefDTO> CreateMediaPlanRef(MediaPlanRefDTO mediaPlanRefDTO)
        {
            await _repository.CreateMediaPlanRef(mediaPlanRefDTO);
            return await _repository.GetMediaPlanRefByIdAndVersion(mediaPlanRefDTO.cmpid, mediaPlanRefDTO.version);
        }

        public async Task<int> GetLatestRefVersionById(int id)
        {
            return await _repository.GetLatestRefVersionById(id);
        }

        public async Task<MediaPlanRefDTO> GetMediaPlanRefByIdAndVersion(int id, int version)
        {
            return await _repository.GetMediaPlanRefByIdAndVersion(id, version);
        }

        public async Task<bool> UpdateMediaPlanRef(MediaPlanRefDTO mediaPlanRefDTO)
        {
            return await _repository.UpdateMediaPlanRef(mediaPlanRefDTO);
        }

        public async Task<bool> DeleteMediaPlanRefById(int id)
        {
            return await _repository.DeleteMediaPlanRefById(id);
        }

        public async Task<bool> DeleteMediaPlanRefByIdAndVersion(int id, int version)
        {
            return await _repository.DeleteMediaPlanRefByIdAndVersion(id, version);
        }
    }
}
