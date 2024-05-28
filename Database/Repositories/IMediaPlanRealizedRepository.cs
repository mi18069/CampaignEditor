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
        Task<bool> UpdateMediaPlanRealized(UpdateMediaPlanRealizedDTO mediaPlanRealizedDTO);
        Task<bool> UpdateMediaPlanRealized(MediaPlanRealized mediaPlanRealized);
        Task<bool> DeleteMediaPlanRealizedById(int id);
        Task<string> GetDedicatedSpotName(int spotid);
        Task<bool> SetStatusValue(int id, int statusValue);
    }
}
