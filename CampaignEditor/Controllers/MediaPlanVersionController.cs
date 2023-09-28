using Database.DTOs.MediaPlanVersionDTO;
using Database.Repositories;
using System;
using System.Threading.Tasks;

namespace CampaignEditor.Controllers
{
    public class MediaPlanVersionController
    {
        private readonly IMediaPlanVersionRepository _repository;
        public MediaPlanVersionController(IMediaPlanVersionRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> CreateMediaPlanVersion(MediaPlanVersionDTO mediaPlanVersionDTO)
        {
            return await _repository.CreateMediaPlanVersion(mediaPlanVersionDTO);
        }

        public async Task<MediaPlanVersionDTO> GetLatestMediaPlanVersion(int cmpid)
        {
            try
            {
                MediaPlanVersionDTO mpVerDTO = await _repository.GetLatestMediaPlanVersion(cmpid);
                if (mpVerDTO != null)
                {
                    return mpVerDTO;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }

        }

        public async Task<MediaPlanVersionDTO> IncrementMediaPlanVersion(MediaPlanVersionDTO mediaPlanVersionDTO)
        {
            await _repository.IncrementMediaPlanVersion(mediaPlanVersionDTO);
            return await _repository.GetLatestMediaPlanVersion(mediaPlanVersionDTO.cmpid); 
        }

        public async Task<bool> DeleteMediaPlanVersionById(int cmpid)
        {
            return await _repository.DeleteMediaPlanVersionById(cmpid);

        }
    }
}
