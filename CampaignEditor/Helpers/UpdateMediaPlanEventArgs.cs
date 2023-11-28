using Database.Entities;
using System;

namespace CampaignEditor.Helpers
{
    public class UpdateMediaPlanEventArgs : EventArgs
    {
        public MediaPlanTuple MediaPlanTuple { get; }

        public UpdateMediaPlanEventArgs(MediaPlanTuple mediaPlanTuple)
        {
            MediaPlanTuple = mediaPlanTuple;
        }
    }
}
