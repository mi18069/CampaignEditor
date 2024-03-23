using Database.DTOs.MediaPlanDTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IMediaPlanRepository
    {
        Task<bool> CreateMediaPlan(CreateMediaPlanDTO mediaPlanDTO);
        Task<MediaPlanDTO> CreateAndReturnMediaPlan(CreateMediaPlanDTO mediaPlanDTO);
        Task<MediaPlanDTO> GetMediaPlanById(int id);
        Task<MediaPlanDTO> GetMediaPlanBySchemaId(int id);
        Task<MediaPlanDTO?> GetMediaPlanBySchemaAndCmpId(int schemaid, int cmpid, int version);
        Task<MediaPlanDTO> GetMediaPlanByCmpId(int id);
        Task<MediaPlanDTO> GetMediaPlanByName(string name);
        Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansByCmpid(int cmpid, int version);
        Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansByCmpidAndChannel(int cmpid, int chid, int version);
        Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansByCmpidAndChannelAllVersions(int cmpid, int chid);
        Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlans();
        Task<IEnumerable<int>> GetAllChannelsByCmpid(int cmpid, int version);
        Task<IEnumerable<MediaPlanDTO>> GetAllChannelCmpMediaPlans(int chid, int cmpid, int version);
        Task<IEnumerable<MediaPlanDTO>> GetAllMediaPlansWithinDate(DateOnly sdate, DateOnly edate);
        Task<bool> UpdateMediaPlan(UpdateMediaPlanDTO mediaPlanDTO);
        Task<bool> DeleteMediaPlanById(int id);
        Task<bool> DeleteMediaPlanByCmpId(int id);
        Task<bool> SetActiveMediaPlanById(int id, bool isActive);

    }
}
