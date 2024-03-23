using System;

namespace CampaignEditor.Helpers
{
    public class UserContextMenuEventArgs : EventArgs
    {
        public enum Options { NewClient = 0 };
        public Enum Option { get; private set; }
        public UserContextMenuEventArgs(Options option)
        {
            Option = option;
        }
    }
}
