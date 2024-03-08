using Database.Entities;
using System;

namespace CampaignEditor.Helpers
{
    public class UpdatedTermEventArgs : EventArgs
    {
        public MediaPlanTerm Term { get; set; }
        public char? Spotcode { get; set; }
        public UpdatedTermEventArgs(MediaPlanTerm term, char? spotcode = null) 
        {
            Term = term;
            Spotcode = spotcode;
        }   
    }
}
