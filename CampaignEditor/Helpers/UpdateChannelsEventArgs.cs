using System.Collections.Generic;

namespace CampaignEditor.Helpers
{
    public class UpdateChannelsEventArgs
    {
        public List<int> channelsToDelete = new List<int>();
        public List<int> channelsToAdd = new List<int>();

        public UpdateChannelsEventArgs(List<int> channelsToDelete, List<int> channelsToAdd)
        {
            this.channelsToDelete = channelsToDelete;
            this.channelsToAdd = channelsToAdd;
        }
    }
}
