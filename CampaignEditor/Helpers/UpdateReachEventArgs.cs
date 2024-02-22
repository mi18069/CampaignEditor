using Database.DTOs.ReachDTO;
using System;

namespace CampaignEditor.Helpers
{
    public class UpdateReachEventArgs : EventArgs
    {
        public ReachDTO Reach { get; }

        public UpdateReachEventArgs(ReachDTO reach)
        {
            Reach = reach;
        }
    }
}
