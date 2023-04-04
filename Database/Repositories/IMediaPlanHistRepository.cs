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
        Task<MediaPlanHistDTO> GetMediaPlanHistByName(string name);
        Task<IEnumerable<MediaPlanHistDTO>> GetAllMediaPlanHists();
        Task<IEnumerable<MediaPlanHistDTO>> GetAllChannelMediaPlanHists(int chid);
        Task<IEnumerable<MediaPlanHistDTO>> GetAllMediaPlanHistsWithinDate(DateOnly sdate, DateOnly edate);
        Task<bool> UpdateMediaPlanHist(UpdateMediaPlanHistDTO mediaPlanHistDTO);
        Task<bool> DeleteMediaPlanHistById(int id);
    }
}
