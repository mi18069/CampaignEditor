using Database.Entities;
using System;

namespace CampaignEditor.Helpers
{
    public class RealizationsAcquiredEventArgs
    {
        public ObservableRangeCollection<MediaPlanRealized> MediaPlanRealized;
        public DateOnly LastImportedDate;

        public RealizationsAcquiredEventArgs(ObservableRangeCollection<MediaPlanRealized> mediaPlanRealized, DateOnly lastImportedDate)
        {
            MediaPlanRealized = mediaPlanRealized;
            LastImportedDate = lastImportedDate;
        }
    }
}
