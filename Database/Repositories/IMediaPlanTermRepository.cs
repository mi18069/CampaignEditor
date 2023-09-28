﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.DTOs.MediaPlanTermDTO;

namespace Database.Repositories
{
    public interface IMediaPlanTermRepository
    {
        Task<bool> CreateMediaPlanTerm(CreateMediaPlanTermDTO mediaPlanTermDTO);
        Task<MediaPlanTermDTO> GetMediaPlanTermById(int id);
        Task<MediaPlanTermDTO> GetMediaPlanTermByXmpidAndDate(int id, DateOnly date);
        Task<IEnumerable<MediaPlanTermDTO>> GetAllMediaPlanTerms();
        Task<IEnumerable<MediaPlanTermDTO>> GetAllMediaPlanTermsByXmpid(int xmpid);
        Task<bool> UpdateMediaPlanTerm(UpdateMediaPlanTermDTO mediaPlanTermDTO);
        Task<bool> DeleteMediaPlanTermById(int id);
        Task<bool> DeleteMediaPlanTermByXmpId(int id);
        Task<bool> SetActiveMediaPlanTermByMPId(int id, bool isActive);

    }
}
