using Database.DTOs.MediaPlanRef;
using Database.Entities;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
            return await _repository.GetMediaPlanRef(mediaPlanRefDTO.cmpid);
        }

        public async Task<MediaPlanRefDTO> GetMediaPlanRef(int id)
        {
            return await _repository.GetMediaPlanRef(id);
        }

        public async Task<IEnumerable<MediaPlanRefDTO>> GetAllMediaPlanRefsByCmpid(int cmpid)
        {
            return await _repository.GetAllMediaPlanRefsByCmpid(cmpid);
        }

        public async Task<bool> UpdateMediaPlanRef(MediaPlanRefDTO mediaPlanRefDTO)
        {
            return await _repository.UpdateMediaPlanRef(mediaPlanRefDTO);
        }

        public async Task<bool> DeleteMediaPlanRefById(int id)
        {
            return await _repository.DeleteMediaPlanRefById(id);
        }

    }
}
