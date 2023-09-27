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
                    MediaPlanVersionDTO mpVersionDTO = new MediaPlanVersionDTO(cmpid, 1);
                    await _repository.CreateMediaPlanVersion(mpVersionDTO);
                    return await _repository.GetLatestMediaPlanVersion(cmpid);
                }
            }
            catch
            {
                MediaPlanVersionDTO mpVersionDTO = new MediaPlanVersionDTO(cmpid, 1);
                await _repository.CreateMediaPlanVersion(mpVersionDTO);
                return await _repository.GetLatestMediaPlanVersion(cmpid);
            }

        }

        public async Task<bool> IncrementMediaPlanVersion(MediaPlanVersionDTO mediaPlanVersionDTO)
        {
           return await _repository.IncrementMediaPlanVersion(mediaPlanVersionDTO);
        }

        public async Task<bool> DeleteMediaPlanVersionById(int cmpid)
        {
            return await _repository.DeleteMediaPlanVersionById(cmpid);

        }
    }
}
