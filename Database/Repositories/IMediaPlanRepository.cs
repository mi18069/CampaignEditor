using Database.DTOs.MediaPlanDTO;
using Database.DTOs.SchemaDTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IMediaPlanRepository
    {
        Task<bool> CreateMediaPlan(CreateMediaPlanDTO mediaPlanDTO);
        Task<MediaPlanDTO> GetMediaPlanById(int id);
        Task<MediaPlanDTO> GetMediaPlanByName(string name);
        Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlans();
        Task<IEnumerable<MediaPlanDTO>> GetAllChannelMediaPlans(int chid);
        Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansWithinDate(DateOnly sdate, DateOnly edate);
        Task<bool> UpdateMediaPlan(UpdateMediaPlanDTO mediaPlanDTO);
        Task<bool> DeleteMediaPlanById(int id);
    }
}
