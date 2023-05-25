using System.Collections.Generic;
using System.Threading.Tasks;
using Database.DTOs.MediaPlanHistDTO;
using Database.Entities;

namespace Database.Repositories
{
    public interface IMediaPlanHistRepository
    {
        Task<bool> CreateMediaPlanHist(CreateMediaPlanHistDTO mediaPlanHistDTO);
        Task<MediaPlanHistDTO> GetMediaPlanHistById(int id);
        Task<IEnumerable<MediaPlanHist>> GetAllMediaPlanHistsByXmpid(int xmpid);
        Task<IEnumerable<MediaPlanHist>> GetAllMediaPlanHistsBySchid(int schid);
        Task<IEnumerable<MediaPlanHistDTO>> GetAllMediaPlanHists();
        Task<IEnumerable<MediaPlanHistDTO>> GetAllChannelMediaPlanHists(int chid);
        Task<bool> UpdateMediaPlanHist(UpdateMediaPlanHistDTO mediaPlanHistDTO);
        Task<bool> DeleteMediaPlanHistById(int id);
        Task<bool> DeleteMediaPlanHistByXmpid(int xmpid);
        MediaPlanHist ConvertFromDTO(MediaPlanHistDTO mediaPlanHistDTO);
        MediaPlanHistDTO ConvertToDTO(MediaPlanHist mediaPlanHist);

    }
}
