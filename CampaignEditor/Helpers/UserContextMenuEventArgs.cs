using CampaignEditor.DTOs.UserDTO;
using System;

namespace CampaignEditor.Helpers
{
    public class UserContextMenuEventArgs : EventArgs
    {
        public enum Options { AllUsers = 0, NewUser = 1, NewClient = 2};
        public Enum Option { get; private set; }
        public UserContextMenuEventArgs(Options option)
        {
            Option = option;
        }
    }
}
