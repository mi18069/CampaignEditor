using Database.DTOs.MediaPlanRef;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IMediaPlanRefRepository
    {
        Task<bool> CreateMediaPlanRef(MediaPlanRefDTO mediaPlanRefDTO);
        Task<MediaPlanRefDTO> GetMediaPlanRef(int id);
        Task<bool> UpdateMediaPlanRef(MediaPlanRefDTO mediaPlanRefDTO);
        Task<bool> DeleteMediaPlanRefById(int id);
    }
}
