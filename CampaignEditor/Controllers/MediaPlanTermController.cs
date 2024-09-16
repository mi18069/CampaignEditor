using Database.DTOs.MediaPlanTermDTO;
using Database.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class MediaPlanTermController : ControllerBase
    {
        private readonly IMediaPlanTermRepository _repository;
        public MediaPlanTermController(IMediaPlanTermRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        public async Task<MediaPlanTermDTO> CreateMediaPlanTerm(CreateMediaPlanTermDTO mediaPlanTermDTO)
        {
            try
            {
                await _repository.CreateMediaPlanTerm(mediaPlanTermDTO);
            }
            catch
            {
                await _repository.SetTermSerialNumber();
                await _repository.CreateMediaPlanTerm(mediaPlanTermDTO);
            }
            return await _repository.GetMediaPlanTermByXmpidAndDate(mediaPlanTermDTO.xmpid, mediaPlanTermDTO.date);
        }

        public async Task<MediaPlanTermDTO> GetMediaPlanTermById(int id)
        {
            return await _repository.GetMediaPlanTermById(id);
        }

        public async Task<IEnumerable<MediaPlanTermDTO>> GetAllMediaPlanTermsByXmpid(int xmpid)
        {
            return await _repository.GetAllMediaPlanTermsByXmpid(xmpid);
        }

        public async Task<IEnumerable<MediaPlanTermDTO>> GetAllNotNullMediaPlanTermsByXmpid(int xmpid)
        {
            return await _repository.GetAllNotNullMediaPlanTermsByXmpid(xmpid);
        }

        public async Task<MediaPlanTermDTO> GetMediaPlanTermByXmpidAndDate(int xmpid, DateOnly date)
        {           
            return await _repository.GetMediaPlanTermByXmpidAndDate(xmpid, date);
        }

        public async Task<bool> CheckIfMediaPlanHasSpotsDedicated(int xmpid)
        {
            return await _repository.CheckIfMediaPlanHasSpotsDedicated(xmpid);
        }

        public async Task<IEnumerable<MediaPlanTermDTO>> GetAllMediaPlanTerms()
        {
            return await _repository.GetAllMediaPlanTerms();
        }

        public async Task<bool> UpdateMediaPlanTerm(UpdateMediaPlanTermDTO mediaPlanTermDTO)
        {
            return await _repository.UpdateMediaPlanTerm(mediaPlanTermDTO);
        }

        public async Task<bool> DeleteMediaPlanTermById(int id)
        {
            return await _repository.DeleteMediaPlanTermById(id);
        }

        public async Task<bool> DeleteMediaPlanTermByXmpId(int id)
        {
            return await _repository.DeleteMediaPlanTermByXmpId(id);
        }
        public async Task<bool> SetActiveMediaPlanTermByMPId(int id, bool isActive)
        {
            return await _repository.SetActiveMediaPlanTermByMPId(id, isActive);
        }
    }
}
