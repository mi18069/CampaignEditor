using Database.Entities;
using System;

namespace CampaignEditor.Helpers
{
    public class UpdateMediaPlanTupleEventArgs : EventArgs
    {
        public MediaPlanTuple MediaPlanTuple { get; }

        public UpdateMediaPlanTupleEventArgs(MediaPlanTuple mediaPlanTuple)
        {
            MediaPlanTuple = mediaPlanTuple;
        }
    }
}
