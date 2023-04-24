using Database.DTOs.MediaPlanRef;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IMediaPlanRefRepository
    {
        Task<bool> CreateMediaPlanRef(MediaPlanRefDTO mediaPlanRefDTO);
        Task<int> GetLatestRefVersionById(int id);
        Task<MediaPlanRefDTO> GetMediaPlanRefByIdAndVersion(int id, int version);
        Task<bool> UpdateMediaPlanRef(MediaPlanRefDTO mediaPlanRefDTO);
        Task<bool> DeleteMediaPlanRefById(int id);
        Task<bool> DeleteMediaPlanRefByIdAndVersion(int id, int version);
    }
}
