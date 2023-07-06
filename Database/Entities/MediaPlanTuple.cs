using Database.DTOs.MediaPlanTermDTO;
using System.Collections.ObjectModel;

namespace Database.Entities
{
    public class MediaPlanTuple
    {
        public MediaPlan MediaPlan { get; set; }
        public ObservableCollection<MediaPlanTerm> Terms { get; set; }

        public MediaPlanTuple(MediaPlan mediaPlan, ObservableCollection<MediaPlanTerm> terms)
        {
            MediaPlan = mediaPlan;
            Terms = terms;
        }
    }
}
