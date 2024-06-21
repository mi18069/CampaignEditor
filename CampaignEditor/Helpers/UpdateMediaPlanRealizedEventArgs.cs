using Database.Entities;

namespace CampaignEditor.Helpers
{
    public class UpdateMediaPlanRealizedEventArgs
    {
        public MediaPlanRealized MediaPlanRealized { get; private set; }
        public bool CoefsUpdated { get; private set; }

        public UpdateMediaPlanRealizedEventArgs(MediaPlanRealized mediaPlanRealized, bool coefsUpdated = false)
        {
            MediaPlanRealized = mediaPlanRealized;
            CoefsUpdated = coefsUpdated;
        }
    }
}
