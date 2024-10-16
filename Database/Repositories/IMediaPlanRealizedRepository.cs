﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database.DTOs.MediaPlanRealizedDTO;
using Database.Entities;

namespace Database.Repositories
{
    public interface IMediaPlanRealizedRepository
    {
        Task<bool> CreateMediaPlanRealized(CreateMediaPlanRealizedDTO mediaPlanRealizedDTO);
        Task<MediaPlanRealizedDTO> GetMediaPlanRealizedById(int id);
        Task<IEnumerable<MediaPlanRealized>> GetAllMediaPlansRealizedByCmpid(int id);
        Task<IEnumerable<MediaPlanRealizedDTO>> GetAllMediaPlansRealizedByChid(int chid);
        Task<IEnumerable<MediaPlanRealizedDTO>> GetAllMediaPlansRealizedByChidAndDate(int chid, string date);
        Task<IEnumerable<MediaPlanRealizedDTO>> GetAllMediaPlansRealizedByCmpidAndEmsnum(int cmpid, int emsnum);
        Task<bool> UpdateMediaPlanRealized(UpdateMediaPlanRealizedDTO mediaPlanRealizedDTO);
        Task<bool> UpdateMediaPlanRealized(MediaPlanRealized mediaPlanRealized);
        Task<bool> UpdatePriceMediaPlanRealized(int id, decimal price);
        Task<bool> DeleteMediaPlanRealizedById(int id);
        Task<bool> DeleteMediaPlanRealizedForCampaign(int cmpid);
        Task<string> GetDedicatedSpotName(int spotid);
        Task<List<Tuple<int, string>>> GetAllSpotNumSpotNamePairs(int cmpid);

        Task<bool> SetStatusValue(int id, int statusValue);
        Task<bool> SetAcceptValue(int id, bool acceptValue);
    }
}
