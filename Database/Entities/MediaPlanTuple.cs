using Database.DTOs.MediaPlanDTO;
using Database.DTOs.MediaPlanTermDTO;
using System.Collections.ObjectModel;

namespace Database.Entities
{
    public class MediaPlanTuple
    {
        public MediaPlanDTO MediaPlan { get; set; }
        public ObservableCollection<MediaPlanTermDTO> Terms { get; set; }

        public MediaPlanTuple(MediaPlanDTO mediaPlan, ObservableCollection<MediaPlanTermDTO> terms)
        {
            MediaPlan = mediaPlan;
            Terms = terms;
        }
    }
}
