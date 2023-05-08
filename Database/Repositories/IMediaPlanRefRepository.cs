using Database.DTOs.MediaPlanRef;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IMediaPlanRefRepository
    {
        Task<bool> CreateMediaPlanRef(MediaPlanRefDTO mediaPlanRefDTO);
        Task<MediaPlanRefDTO> GetMediaPlanRef(int id);
        Task<IEnumerable<MediaPlanRefDTO>> GetAllMediaPlanRefsByCmpid(int cmpid);
        Task<bool> UpdateMediaPlanRef(MediaPlanRefDTO mediaPlanRefDTO);
        Task<bool> DeleteMediaPlanRefById(int id);
    }
}
