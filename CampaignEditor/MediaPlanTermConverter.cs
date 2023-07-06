using AutoMapper;
using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanTermDTO;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignEditor
{
    public class MediaPlanTermConverter
    {
        private readonly IMapper _mapper;

        public MediaPlanTermConverter(IMapper mapper)
        {
            _mapper = mapper;
        }

        public MediaPlanTerm ConvertFromDTO(MediaPlanTermDTO mediaPlanTermDTO)
        {

            var mediaPlanTerm = _mapper.Map<MediaPlanTerm>(mediaPlanTermDTO);

            return mediaPlanTerm;
        }

        public MediaPlanTermDTO ConvertToDTO(MediaPlanTerm mediaPlanTerm)
        {
            MediaPlanTermDTO mediaPlanTermDTO = new MediaPlanTermDTO(
                mediaPlanTerm.Xmptermid, mediaPlanTerm.Xmpid, mediaPlanTerm.Date, mediaPlanTerm.Spotcode);

            return mediaPlanTermDTO;
        }
    }

}
