using System.Collections;
using System.Collections.Generic;

namespace CampaignEditor.Helpers
{
    // This class is used in MediaPlanGrid, when we want to make order by given list of channels
    public class CustomChidComparer : IComparer
    {
        private readonly List<int> _orderedChids;

        public CustomChidComparer(List<int> orderedChids)
        {
            _orderedChids = orderedChids;
        }

        public int Compare(object x, object y)
        {
            // Get the MediaPlan from the MediaPlanTuple
            var xMediaPlan = x.GetType().GetProperty("MediaPlan").GetValue(x);
            var yMediaPlan = y.GetType().GetProperty("MediaPlan").GetValue(y);

            // Get the chid from the MediaPlan
            int xChid = (int)xMediaPlan.GetType().GetProperty("chid").GetValue(xMediaPlan);
            int yChid = (int)yMediaPlan.GetType().GetProperty("chid").GetValue(yMediaPlan);

            int indexX = _orderedChids.IndexOf(xChid);
            int indexY = _orderedChids.IndexOf(yChid);

            // Handle cases where the chid is not in the list
            if (indexX == -1) indexX = int.MaxValue;
            if (indexY == -1) indexY = int.MaxValue;

            return indexX.CompareTo(indexY);
        }
    }
}
