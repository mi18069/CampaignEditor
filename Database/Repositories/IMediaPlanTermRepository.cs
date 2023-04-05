using System.Collections.Generic;
using System.Threading.Tasks;
using Database.DTOs.MediaPlanTermDTO;

namespace Database.Repositories
{
    public interface IMediaPlanTermRepository
    {
        Task<bool> CreateMediaPlanTerm(CreateMediaPlanTermDTO mediaPlanTermDTO);
        Task<MediaPlanTermDTO> GetMediaPlanTermById(int id);
        Task<IEnumerable<MediaPlanTermDTO>> GetAllMediaPlanTerms();
        Task<IEnumerable<MediaPlanTermDTO>> GetAllMediaPlanTermsByXmpid(int xmpid);
        Task<bool> UpdateMediaPlanTerm(UpdateMediaPlanTermDTO mediaPlanTermDTO);
        Task<bool> DeleteMediaPlanTermById(int id);
    }
}
