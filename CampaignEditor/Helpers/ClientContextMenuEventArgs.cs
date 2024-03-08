using Database.DTOs.ClientDTO;
using System;

namespace CampaignEditor.Helpers
{
    public class ClientContextMenuEventArgs : EventArgs
    {
        public ClientDTO Client { get; private set; }
        public enum Options { UsersOfClient = 0, NewCampaign = 1, RenameClient = 2, DeleteClient = 3 };

        public Enum Option { get; private set; } 
        public ClientContextMenuEventArgs(ClientDTO client, Options options)
        {
            Client = client;
            Option = options;
        }
    }
}
