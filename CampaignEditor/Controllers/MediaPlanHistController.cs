﻿using Database.DTOs.MediaPlanHistDTO;
using Database.Entities;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class MediaPlanHistController : ControllerBase
    {
        private readonly IMediaPlanHistRepository _repository;
        public MediaPlanHistController(IMediaPlanHistRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<bool> CreateMediaPlanHist(CreateMediaPlanHistDTO mediaPlanHistDTO)
        {
            return await _repository.CreateMediaPlanHist(mediaPlanHistDTO);
        }

        public async Task<MediaPlanHistDTO> GetMediaPlanHistById(int id)
        {
            return await _repository.GetMediaPlanHistById(id);
        }

        public async Task<IEnumerable<MediaPlanHist>> GetAllMediaPlanHistsByXmpid(int xmpid)
        {
            return await _repository.GetAllMediaPlanHistsByXmpid(xmpid);
        }

        public async Task<IEnumerable<MediaPlanHist>> GetAllMediaPlanHistsBySchid(int schid)
        {
            return await _repository.GetAllMediaPlanHistsBySchid(schid);
        }

        public async Task<IEnumerable<MediaPlanHistDTO>> GetAllMediaPlanHists()
        {
            return await _repository.GetAllMediaPlanHists();
        }

        public async Task<IEnumerable<MediaPlanHistDTO>> GetAllChannelMediaPlanHists(int chid)
        {
            return await _repository.GetAllChannelMediaPlanHists(chid);
        }

        public async Task<bool> UpdateMediaPlanHist(UpdateMediaPlanHistDTO mediaPlanHistDTO)
        {
            return await _repository.UpdateMediaPlanHist(mediaPlanHistDTO);
        }

        public async Task<bool> DeleteMediaPlanHistById(int id)
        {
            return await _repository.DeleteMediaPlanHistById(id);
        }

        public async Task<bool> DeleteMediaPlanHistByXmpid(int xmpid)
        {
            return await _repository.DeleteMediaPlanHistByXmpid(xmpid);
        }

        public async Task<bool> SetActiveMediaPlanHistByMPId(int id, bool isActive)
        {
            return await _repository.SetActiveMediaPlanHistByMPId(id, isActive);
        }

        public MediaPlanHist ConvertFromDTO(MediaPlanHistDTO mediaPlanHistDTO)
        {
            return _repository.ConvertFromDTO(mediaPlanHistDTO);
        }

        public MediaPlanHistDTO ConvertToDTO(MediaPlanHist mediaPlanHist)
        {
            return _repository.ConvertToDTO(mediaPlanHist);
        }

    }
}
