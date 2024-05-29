using System;

namespace CampaignEditor.Helpers
{
    public class IndexEventArgs : EventArgs
    {
        public int Index { get; private set; }

        public IndexEventArgs(int index)
        {
            Index = index;
        }
    }
}
