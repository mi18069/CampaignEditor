using Database.Entities;

namespace CampaignEditor.Helpers
{
    public class UpdateMediaPlanRealizedEventArgs
    {
        public MediaPlanRealized MediaPlanRealized { get; private set; }
        public bool RecalculateCoefs { get; private set; }
        public bool RecalculatePrice { get; private set; }

        public UpdateMediaPlanRealizedEventArgs(MediaPlanRealized mediaPlanRealized, bool recalculateCoefs = false, bool recalculatePrice = false)
        {
            MediaPlanRealized = mediaPlanRealized;
            RecalculateCoefs = recalculateCoefs;
            RecalculatePrice = recalculatePrice;
        }
    }
}
