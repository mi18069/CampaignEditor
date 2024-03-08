using Database.DTOs.MediaPlanVersionDTO;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IMediaPlanVersionRepository
    {
        Task<bool> CreateMediaPlanVersion(MediaPlanVersionDTO mediaPlanVersionDTO);
        Task<MediaPlanVersionDTO> GetLatestMediaPlanVersion(int cmpid);
        Task<bool> IncrementMediaPlanVersion(MediaPlanVersionDTO mediaPlanVersionDTO);
        Task<bool> UpdateMediaPlanVersion(int cmpid, int version);
        Task<bool> DeleteMediaPlanVersionById(int cmpid);
    }
}
