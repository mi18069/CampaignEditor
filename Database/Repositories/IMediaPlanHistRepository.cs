using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Database.DTOs.MediaPlanHistDTO;

namespace Database.Repositories
{
    public interface IMediaPlanHistRepository
    {
        Task<bool> CreateMediaPlanHist(CreateMediaPlanHistDTO mediaPlanHistDTO);
        Task<MediaPlanHistDTO> GetMediaPlanHistById(int id);
        Task<IEnumerable<MediaPlanHistDTO>> GetAllMediaPlanHistsByXmpid(int xmpid);
        Task<IEnumerable<MediaPlanHistDTO>> GetAllMediaPlanHistsBySchid(int schid);
        Task<IEnumerable<MediaPlanHistDTO>> GetAllMediaPlanHists();
        Task<IEnumerable<MediaPlanHistDTO>> GetAllChannelMediaPlanHists(int chid);
        Task<bool> UpdateMediaPlanHist(UpdateMediaPlanHistDTO mediaPlanHistDTO);
        Task<bool> DeleteMediaPlanHistById(int id);
        Task<bool> DeleteMediaPlanHistByXmpid(int xmpid);
    }
}
