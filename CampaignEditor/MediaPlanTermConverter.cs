using AutoMapper;
using Database.DTOs.MediaPlanTermDTO;
using Database.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                mediaPlanTerm.Xmptermid, mediaPlanTerm.Xmpid, mediaPlanTerm.Date, mediaPlanTerm.Spotcode, mediaPlanTerm.Added, mediaPlanTerm.Deleted);

            return mediaPlanTermDTO;
        }

        public IEnumerable<MediaPlanTermDTO> ConvertToEnumerableDTO(IEnumerable<MediaPlanTerm?> mediaPlanTerms)
        {
            return _mapper.Map<IEnumerable<MediaPlanTermDTO>>(mediaPlanTerms);
        }

        public string? CalculateMpTermAdded(string? newSpotcode, string? oldSpotcode)
        {
            if (string.IsNullOrEmpty(newSpotcode))
                return null;

            if (string.IsNullOrEmpty(oldSpotcode))
                return newSpotcode;

            StringBuilder sb = new StringBuilder("");
            foreach (char c in newSpotcode.Trim())
            {
                // if oldSpotcode contains char, delete it and continue
                if (oldSpotcode.Contains(c))
                {
                    int index = oldSpotcode.IndexOf(c);
                    oldSpotcode = oldSpotcode.Remove(index, 1);
                }
                // else add that char to added
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Count() == 0 ? null : sb.ToString();
        }

        public string? CalculateMpTermDeleted(string? newSpotcode, string? oldSpotcode)
        {
            if (string.IsNullOrEmpty(newSpotcode))
                return oldSpotcode;

            if (string.IsNullOrEmpty(oldSpotcode))
                return null;

            StringBuilder sb = new StringBuilder("");
            foreach (char c in oldSpotcode.Trim())
            {
                // if newSpotcode contains char, delete it and continue
                if (newSpotcode.Contains(c))
                {
                    int index = newSpotcode.IndexOf(c);
                    newSpotcode = newSpotcode.Remove(index, 1);
                }
                // else add that char to deleted
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Count() == 0 ? null : sb.ToString();
        }
    }

}
