using Database.Entities;
using System;

namespace CampaignEditor.Helpers
{
    public class UpdateMediaPlanEventArgs : EventArgs
    {
        public MediaPlan MediaPlan { get; }

        public UpdateMediaPlanEventArgs(MediaPlan mediaPlan)
        {
            MediaPlan = mediaPlan;
        }
    }
}
