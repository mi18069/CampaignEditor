using System;

namespace CampaignEditor.Helpers
{
    public class ChangeVersionEventArgs : EventArgs
    {
        public int Version { get; }
        public bool Change { get; }
        public ChangeVersionEventArgs(int version)
        {
            Version = version;
        }
    }
}
