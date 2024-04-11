using System.Collections.Generic;
using System.Threading.Tasks;
using Database.DTOs.MediaPlanRealizedDTO;

namespace Database.Repositories
{
    public interface IMediaPlanRealizedRepository
    {
        Task<bool> CreateMediaPlanRealized(CreateMediaPlanRealizedDTO mediaPlanRealizedDTO);
        Task<MediaPlanRealizedDTO> GetMediaPlanRealizedById(int id);
        Task<IEnumerable<MediaPlanRealizedDTO>> GetAllMediaPlansRealizedByChid(int chid);
        Task<IEnumerable<MediaPlanRealizedDTO>> GetAllMediaPlansRealizedByChidAndDate(int chid, string date);
        Task<bool> UpdateMediaPlanRealized(UpdateMediaPlanRealizedDTO mediaPlanRealizedDTO);
        Task<bool> DeleteMediaPlanRealizedById(int id);
    }
}
